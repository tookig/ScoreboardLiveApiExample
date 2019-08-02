﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using ScoreboardLiveApi;
using System.Net;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace ScoreboardLiveApiExample {
  class Test {
    private enum CheckDeviceResult { Valid, Invalid, Error }

    private static readonly HttpClient client = new HttpClient();
    private static readonly Routes routes = new Routes("http://192.168.2.102/monkeyscore");

    private static ApiHelper api = new ApiHelper("http://192.168.2.102/monkeyscore");
    private static readonly string keyStoreFile = string.Format("{0}scoreboardTestAppKeys.bin", AppDomain.CurrentDomain.BaseDirectory);

    public static string ByteArrayToString(byte[] ba) {
      StringBuilder hex = new StringBuilder(ba.Length * 2);
      foreach (byte b in ba)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }


    private Test() {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      client.DefaultRequestHeaders.Add("User-Agent", "Scoreboard Live API Tester");
}

    private static void PrintResponse(HttpStatusCode statusCode) {
      PrintResponse(statusCode, null);
    }

    private static void PrintResponse(HttpStatusCode statusCode, ScoreboardResponse sbResponse) {
      Console.WriteLine("The request returned with status code {0}.", statusCode);
      if ((sbResponse != null) && (sbResponse.Success == 0)) {
        Console.WriteLine("The server responded with a failure. The following errors occured:");
        foreach (string error in sbResponse.Errors) {
          Console.WriteLine(" - {0}", error);
        }
      }
    }

    private static void PrintResponse(ScoreboardResponse sbResponse) {
      if ((sbResponse != null) && (sbResponse.Success == 0)) {
        Console.WriteLine("The server responded with a failure. The following errors occured:");
        foreach (string error in sbResponse.Errors) {
          Console.WriteLine(" - {0}", error);
        }
        Console.WriteLine();
      }
    }

    private static void PrintError(Exception e) {
      Console.WriteLine("Network operation error:");
      Console.WriteLine(e.Message);
      Console.WriteLine();
    }

    private async Task<List<Unit>> GetUnits() {
      var serializer = new DataContractJsonSerializer(typeof(Unit.UnitResponse));
      var streamTask = client.GetStreamAsync(routes.GetUnits());
      var unitResponse = serializer.ReadObject(await streamTask) as Unit.UnitResponse;
      return unitResponse.Units;
    }

    private async Task<Dictionary<Unit, Device>> AssociateUnitsWithDeviceRegistrations(List<Unit> units) {
      // Load the keystore
      LocalKeyStore keyStore = LocalKeyStore.Load(keyStoreFile);
      // For each of the units supplied, check if there is a saved device, and in  that case
      // use the API test function to make sure the server still think it is valid. If all tests
      // are positive, pair the two.
      Dictionary<Unit, Device> pairs = new Dictionary<Unit, Device>();
      foreach (Unit unit in units) {
        // Find in key store
        Device device = keyStore.Get(unit.UnitID);
        // If found, check agains server. If server rejects this device, don't use it
        if (device != null) {
          CheckDeviceResult result = await CheckDevice(device);
          if (result == CheckDeviceResult.Invalid) {
            keyStore.Remove(device);
            device = null;
          }
          else if (result == CheckDeviceResult.Error) {
            device = null;
          }
        }
        // Pair unit with the device
        pairs.Add(unit, device);
      }
      // Save changes to the keystore
      keyStore.Save(keyStoreFile);
      // Return the pairs
      return pairs;
    }

    private async Task<CheckDeviceResult> CheckDevice(Device device) {
      // Create some random http data. Some random content is needed on the request so that
      // the same generated HMAC isn't sent multiple times (this could be a security issue). 
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "randomStuff", Guid.NewGuid().ToString("n") }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Create the HMAC from the http content
      string hash;
      using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(device.ClientToken))) {
        hash = device.DeviceCode + "x" +  ByteArrayToString(hmac.ComputeHash(await content.ReadAsByteArrayAsync()));
      }
      // Create the request
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, routes.CheckDeviceRegistration());
      request.Headers.Add("Authorization", hash);
      request.Content = content;
      // Send request
      Console.WriteLine("Checking if device code {0} is still valid on server", device.DeviceCode);
      Console.WriteLine("Calculated HMAC: {0}", hash);
      Console.WriteLine("Calculated from: {0}", await content.ReadAsStringAsync());
      HttpResponseMessage response = await client.SendAsync(request);
      // Check response
      var serializer = new DataContractJsonSerializer(typeof(ScoreboardResponse));
      ScoreboardResponse scoreboardResponse = serializer.ReadObject(await response.Content.ReadAsStreamAsync()) as ScoreboardResponse;
      PrintResponse(response.StatusCode, scoreboardResponse);
      Console.WriteLine();
      if (response.IsSuccessStatusCode) {
        if (scoreboardResponse.Success == 0) {
          return CheckDeviceResult.Invalid;
        }
        else {
          return CheckDeviceResult.Valid;
        }
      }
      return CheckDeviceResult.Error;
    }

    private Unit SelectUnit(Dictionary<Unit, Device> pairs) {
      Console.WriteLine();
      Console.WriteLine("Select a unit:");
      Console.WriteLine("---------------------------");
      int i = 1;
      Unit[] indexedUnits = new Unit[pairs.Count];
      foreach (KeyValuePair<Unit, Device> kvp in pairs) {
        Console.WriteLine("{0}. {1} ({2})", i, kvp.Key.Name, kvp.Value == null ? "not registered on server" : "registered as " + kvp.Value.DeviceCode);
        indexedUnits[i - 1] = kvp.Key;
        ++i;
      }
      Console.Write("Selection: ");
      int.TryParse(Console.ReadLine(), out int iSelection);
      if ((iSelection < 1) || (iSelection > pairs.Count)) {
        Console.WriteLine("Not a valid selection");
        return SelectUnit(pairs);
      }
      return indexedUnits[iSelection - 1];
    }

    private async Task<Device> RegisterUnit() {
      // Get activation code from user
      Console.WriteLine();
      Console.Write("Enter activation code: ");
      string activationCode = Console.ReadLine().Trim();
      // Create the http data
      Dictionary<string, string> formData = new Dictionary<string, string> {
        { "activationCode", activationCode }
      };
      HttpContent content = new FormUrlEncodedContent(formData);
      // Send request
      Console.WriteLine("Trying to activate using code: {0}", activationCode);
      HttpResponseMessage response = await client.PostAsync(routes.RegisterDevice(), content);
      // Check response
      var serializer = new DataContractJsonSerializer(typeof(Device.DeviceResponse));
      Device.DeviceResponse deviceInfo = serializer.ReadObject(await response.Content.ReadAsStreamAsync()) as Device.DeviceResponse;
      PrintResponse(response.StatusCode, deviceInfo);
      if (response.IsSuccessStatusCode && (deviceInfo.Success != 0)) {
        LocalKeyStore keyStore = LocalKeyStore.Load(keyStoreFile);
        keyStore.Set(deviceInfo.Device);
        keyStore.Save(keyStoreFile);
        return deviceInfo.Device;
      }
      return null;
    }


    static void Main(string[] args) {
      try {
        // First try and fetch all units to select from at server
        try {
          Console.WriteLine("Fetching all available units from server");
          Unit.UnitResponse units = api.GetUnits().Result;
        } catch (Exception e) {
        }

/*
        Dictionary<Unit, Device> pairs = program.AssociateUnitsWithDeviceRegistrations(units).Result;
        Unit selectedUnit = program.SelectUnit(pairs);
        Console.WriteLine("Selected unit: {0}", selectedUnit);
        Device selectedDevice = pairs[selectedUnit];
        if (selectedDevice == null) {
          Console.WriteLine("This device is not registered with the unit {0}.", selectedUnit.Name);
          selectedDevice = program.RegisterUnit().Result;
          if (selectedDevice == null) {
            Console.WriteLine("Registration failed");
          }
          else {
            Console.WriteLine("Registration successful. New device:");
            Console.WriteLine(selectedDevice);
          }
        }
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
      }
      */
    }
  }
}