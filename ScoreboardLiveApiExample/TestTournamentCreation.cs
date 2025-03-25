using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScoreboardLiveApi;
using System.Security.Cryptography;
using System.Text;

namespace ScoreboardLiveApiExample {
  class TestTournamentCreation {
    private static ApiHelper api = new ApiHelper("http://192.168.100.10:9000");
    private static readonly string keyStoreFile = string.Format("{0}scoreboardTestDomainAppKeys.bin", AppDomain.CurrentDomain.BaseDirectory);

    static async Task<Unit> SelectUnit() {
      List<Unit> units = new List<Unit>();
      // First try and fetch all units to select from at server
      Console.Clear();
      Console.WriteLine("Fetching all available units from server");
      try {
        units.AddRange(await api.GetUnits());
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        return null;
      }
      // Print them out
      int i = 1;
      units.ForEach(unit => Console.WriteLine("{0}. {1}", i++, unit.Name));
      // Have the user select one
      Console.Write("Select a unit to use: ");
      int.TryParse(Console.ReadLine(), out int selection);
      // Check so that the number is valid
      if (selection < 1 || selection > units.Count) {
        return await SelectUnit();
      }
      return units[selection - 1];
    }

    static async Task<Device> RegisterWithUnit(Unit unit, LocalDomainKeyStore keyStore) {
      // Get activation code from user
      Console.Clear();
      Console.WriteLine("This device is not registered with {0}.", unit.Name);
      Console.Write("Enter activation code for {0}: ", unit.Name);
      string activationCode = Console.ReadLine().Trim();
      // Register this code with the server
      Device deviceCredentials = null;
      try {
        deviceCredentials = await api.RegisterDevice(activationCode);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        Console.ReadKey();
      }
      // If registration was a success, save the credentials to the key store
      if (deviceCredentials != null) {
        keyStore.Set(deviceCredentials);
        keyStore.Save(keyStoreFile);
      }
      return deviceCredentials;
    }

    static async Task<bool> CheckCredentials(Unit unit, Device device, LocalDomainKeyStore keyStore) {
      Console.WriteLine("Checking so that the credentials on file for {0} are still valid...", unit.Name);
      bool valid = false;
      try {
        valid = await api.CheckCredentials(device);
      } catch (Exception e) {
        // We end up here if it cant be determined if the credentials are valid or not,
        // so don't discard the keys here, just return.
        Console.WriteLine(e.Message);
        return false; 
      }
      if (valid) {
        Console.WriteLine("Credentials checked out OK.");
      } else {
        Console.WriteLine("Credentials no longer valid. Removing from key store.");
        keyStore.Remove(device);
        keyStore.Save(keyStoreFile);
      }
      return valid;
    }


    public static async Task RunTest() {
      api.Error += (sender, e) => Console.WriteLine(e);

      // Select a unit
      Unit selectedUnit = SelectUnit().Result;
      if (selectedUnit == null) return;
      Console.WriteLine("Unit {0} was selected.", selectedUnit.Name);

      // Load the keystore, and select the appropriate key to use for this unit.
      LocalDomainKeyStore keyStore;
      try {
        keyStore = LocalDomainKeyStore.Load(keyStoreFile);
      } catch (Exception e) {
        Console.WriteLine("Could not read the key store file: {0}", e.Message);
        Console.WriteLine("Creating a new key store. Press any key to continue.");
        Console.ReadLine();
        keyStore = new LocalDomainKeyStore();
      }
      // Set the current domain for the key store
      keyStore.DefaultDomain = api.BaseUrl;
      // If this device is not registered with that unit, do registration
      while (keyStore.Get(selectedUnit.UnitID) == null) {
        RegisterWithUnit(selectedUnit, keyStore).Wait();
      }

      // Check the credentials to make sure they are still valid
      Device deviceCredentials = keyStore.Get(selectedUnit.UnitID);
      if (!CheckCredentials(selectedUnit, deviceCredentials, keyStore).Result) {
        Console.ReadKey();
        return;
      }

      try {
        // Create a multiseries tournament
        var newMultiseriesTournament = await api.CreateMultiSeriesTournament(deviceCredentials, $"Test tournament {DateTime.Now.ToString()}", ScoreSystem.FiveSet11Max15, "Europe/Stockholm",
          DateTime.Now.AddDays(-2), DateTime.Now.AddDays(3));
        Console.WriteLine("Multiseries tournament created; assigned ID {0}", newMultiseriesTournament.TournamentID);

        // Add a series tournament to the previously created multiseries
        var seriesTournament = await api.CreateSeriesTournament(deviceCredentials, newMultiseriesTournament, DateTime.Now,
                                                                RandomStuff.TeamName(), RandomStuff.TeamName(),
                                                                new ScoreboardApiLib.Helpers.SeriesSetup(1, 2, 3, 0, 1));
        Console.WriteLine("Series sub-tournament created; assigned ID {0}, {1}-{2}", seriesTournament.TournamentID, seriesTournament.Team1, seriesTournament.Team2);
      } catch (Exception e) {
        Console.WriteLine(e);
      }

      Console.WriteLine();
      Console.WriteLine("Done.");

      Console.ReadKey();
    }

    private static void Api_Error(object sender, ApiHelperErrorEventArgs e) {
      throw new NotImplementedException();
    }
  }
}