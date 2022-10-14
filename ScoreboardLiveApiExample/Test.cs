using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScoreboardLiveApi;

namespace ScoreboardLiveApiExample {
  class Test {
    private static ApiHelper api = new ApiHelper("https://dosan.scoreboardlive.se");
    private static readonly string keyStoreFile = string.Format("{0}scoreboardTestAppKeys.bin", AppDomain.CurrentDomain.BaseDirectory);

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

    static async Task<Device> RegisterWithUnit(Unit unit, LocalKeyStore keyStore) {
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

    static async Task<bool> CheckCredentials(Unit unit, Device device, LocalKeyStore keyStore) {
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

    static async Task<Tournament> SelectTournament(Unit unit, Device device) {
      Console.Clear();
      Console.WriteLine("Downloading latest tournaments for {0}...", unit.Name);
      List<Tournament> tournaments = new List<Tournament>();
      try {
        // Get the 10 newest tournaments for this unit
        tournaments.AddRange(await api.GetTournaments(device, 10));
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        return null;
      }
      // Select a list for the user to choose from
      Console.WriteLine();
      Console.WriteLine("#   Starts     Tournament");
      Console.WriteLine("---------------------------------------------------------------------------");
      int i = 1;
      foreach (Tournament tournament in tournaments) {
        Console.Write("{0,2}. ", i++);
        Console.Write(tournament.StartDate.ToShortDateString());
        if (tournament.TournamentType == "series") {
          Console.Write(" {0} - {1}", tournament.Team1, tournament.Team2);
        } else {
          Console.Write(" {0}", tournament.Name);
        }
        Console.WriteLine(" ({0})", tournament.TournamentType); 
      }
      Console.Write("Select a tournament (leave empty to let server decide): ");
      int.TryParse(Console.ReadLine(), out int selection);
      // Return the selected tournament
      if (selection == 0) {
        return null;
      } else if ((selection > 0) && (selection <= tournaments.Count)) {
        return tournaments[selection - 1];
      }
      return await SelectTournament(unit, device);
    }

    static async Task<Match> CreateRandomMatch(Device device, Tournament tournament) {
      // Create a random match
      Console.Clear();
      Console.WriteLine("Creating a random match and uploading to server...");
      Match match = RandomStuff.RandomMatch();
      // Send request
      Match serverMatch = null;
      try {
        serverMatch = await api.CreateOnTheFlyMatch(device, tournament, match);
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        return null;
      }
      Console.WriteLine("The following match was created:");
      Console.WriteLine(serverMatch);
      Console.WriteLine();
      return serverMatch;
    }

    static async Task<Court> SelectCourt(Device device) {
      // Get courts from server
      Console.Clear();
      Console.WriteLine("Fetching all available courts from server...");
      List<Court> courts = new List<Court>();
      try {
         courts.AddRange(await api.GetCourts(device));
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        return null;
      }
      // List them for user to select
      int i = 1;
      foreach (Court court in courts) {
        Console.WriteLine("{0}. {1} ({2})", i++, court.Name, court.Venue.Name);
      }
      // Get user input
      Console.Write("Select a court: ");
      int.TryParse(Console.ReadLine(), out int selection);
      if ((selection < 1) || (selection > courts.Count)) {
        return await SelectCourt(device);
      }
      return courts[selection - 1];
    }

    static async Task AssignMatchToCourt(Device device, Match match, Court court) {
      Console.WriteLine();
      Console.WriteLine("Assigning match {0} to court {1}", match.MatchID, court.Name);
      try {
        await api.AssignMatchToCourt(device, match, court);
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
      }
    }

    static async Task FindMatchOnServerUsingMatchnumber(Device device, Tournament tournament, int tournamentMatchNumber) {
      Console.WriteLine();
      Console.WriteLine("Trying to find match with tournament match number {0} on server", tournamentMatchNumber);
      List<Match> matches = new List<Match>();
      try {
        matches.AddRange(await api.FindMatchBySequenceNumber(device, tournament, tournamentMatchNumber));
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
      }
      if (matches.Count > 0) {
        Console.WriteLine("Found {0} match{1}:", matches.Count, matches.Count > 1 ? "es" : "");
        foreach (Match i in matches) {
          Console.WriteLine(i);
        }
      }
      else {
        Console.WriteLine("No match found");
      }
    }

    static async Task FindMatchOnServerUsingTag(Device device, string tag) {
      Console.WriteLine();
      Console.WriteLine("Trying to find match with tag hash: {0}", tag);
      List<Match> matches = new List<Match>();
      try {
        matches.AddRange(await api.FindMatchByTag(device, tag));
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
      if (matches.Count > 0) {
        Console.WriteLine("Found {0} match{1}:", matches.Count, matches.Count > 1 ? "es" : "");
        foreach (Match i in matches) {
          Console.WriteLine(i);
          Console.WriteLine("Match tag: {0}", i.Tag);
        }
      } else {
        Console.WriteLine("No match found");
      }
    }

    static void Main(string[] args) {
      // Select a unit
      Unit selectedUnit = SelectUnit().Result;
      if (selectedUnit == null) return;
      Console.WriteLine("Unit {0} was selected.", selectedUnit.Name);

      // Load the keystore, and select the appropriate key to use for this unit.
      // If this device is not registered with that unit, do registration
      LocalKeyStore keyStore = LocalKeyStore.Load(keyStoreFile);
      while (keyStore.Get(selectedUnit.UnitID) == null) {
        RegisterWithUnit(selectedUnit, keyStore).Wait();
      }

      // Check the credentials to make sure they are still valid
      Device deviceCredentials = keyStore.Get(selectedUnit.UnitID);
      if (!CheckCredentials(selectedUnit, deviceCredentials, keyStore).Result) {
        Console.ReadKey();
        return;
      }

      // Select a tournament to add matches to
      Tournament selectedTournament = SelectTournament(selectedUnit, deviceCredentials).Result;
      Console.WriteLine("Selected tournament: {0}", selectedTournament);

      // Create a random match
      Match match = CreateRandomMatch(deviceCredentials, selectedTournament).Result;
      if (match == null) {
        Console.ReadKey();
        return;
      }
      Console.WriteLine("Created a match with hash {0}", ApiHelper.HashMatch(match));

      // Get a list of all available courts for the user to select
      Court court = SelectCourt(deviceCredentials).Result;
      if (court == null) {
        Console.ReadKey();
        return;
      }
      Console.WriteLine("Selected court: {0}", court);

      // Assign the new match to the selected court
      AssignMatchToCourt(deviceCredentials, match, court).Wait();

      // Check if we can retrieve the same match again using the two methods
      FindMatchOnServerUsingMatchnumber(deviceCredentials, selectedTournament, match.TournamentMatchNumber).Wait();
      FindMatchOnServerUsingTag(deviceCredentials, ApiHelper.HashMatch(match)).Wait();

      Console.WriteLine();
      Console.WriteLine("Done.");

      Console.ReadKey();
    }
  }
}