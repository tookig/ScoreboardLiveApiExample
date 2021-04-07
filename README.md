# Scoreboard Live API Test
This is a very basic .NET console application to show examples of how to use the Scoreboard Live API. The solution is split into two projects.  The [ScoreboardApiLib](./ScoreboardApiLib) project contains a simple implementation of the most common operations and data objects used when registering and changing data on the server.

The [ApiHelper class](./ScoreboardApiLib/ApiHelper.cs) contains a number of functions to register a device, test a device, add court, create a match etc.

The [ScoreboardLiveApiExample](./ScoreboardLiveApiExample) project contains the user interactions and handles saving/loading keys.

*Note that the API is a work in progress and this is just an example of what can be done with it.* 

For a list of changes check the [Changelog](#Changelog) at the end of this file.

# Scoreboard Live API Reference
The API is REST-based, but uses only GET or POST. The return body is in JSON format.

## Successfull API calls
When an API HTTP request returns as a success, the HTTP status code will always be 200. The returned body will be in JSON format, and will always have the 'success'-member set to 1:
```javascript
{
  success: 1,
  someData: {...}
}
```

## Bad API calls
If an API call fails, the HTTP status code for the return will be 400, 403, 404 or 500 depending on what happened. For 404 and 500, the return value will not be JSON, but a plain HTML standard server response. For the 400 and 403 responses, the return body will be in JSON format, with a description of the error. The 'success'-member is always set to 0, and the errors encountered are found in the 'errors'-array:
```javascript
{
  success: 0,
  errors: [
    'The Tournament ID must be a number greater than 0',
    'The Start date must be in the format \'YYYY-MM-DD\''
  ]
}
```

# Authorization 
Most of the API calls requires authorization. Scoreboard Live uses HMAC to authorize HTTP-requests. Each application, or device as they are called here, must be registered with the Scoreboard Live server to be able to make API-calls that edit data on the server. When the device registers, it will receive an **activation code** (a six character string) and a **client token**. They are both required for authorization. See  [register_device](#register_device). The activation code and client token should be saved by the application, to avoid having to re-register every time it is used.

When an API request requires authorization, the **Authorization**-header must be set. The value should be a string with the first six characters being the **activation code**, and the following being a hex string representation of the HMAC code generated from the **body** of the request being sent, with the **client token** used as a HMAC key. Example of an **Authorization**-header:

```
Authorization: 94T0H86577ef386315bf7c967c40e49a1fb61e05c42dd96e1e2a30bcb78e72d41a4bb5
```

Since the HMAC is generated using only the body of the HTTP-request, the body should always contain some random element; just add some nonsense-named key with a random generated value.

# API functions
## **get_units**
Get all available units (a unit is an organization or a club registered with Scoreboard Live).

* **URL:** /api/unit/get_units
* **Authorization**: none
* **Method**: GET or POST
* **Parameters**: none
* **Returns:**
    ```javascript
    {
      units: [ // List of all registered units
        { 
          unitid: 1,  // Unit id number
          name: '...' // Name of unit
        }
      ]
    }
    ```
---

## **get_tournaments**
Get all active tournaments for a unit, with the most recent first.

* **URL:** /api/unit/get_tournaments
* **Authorization**: HMAC
* **Method**: POST
* **Parameters**:
   * **limit** (optional): max number of tournaments to return.
* **Returns:**
    ```javascript
    {
      tournaments: [...]
    }
    ```
---

## **get_tournament**
Get information on a specific tournament.

* **URL:** /api/tournament/get_tournament
* **Authorization**: none
* **Method**: POST
* **Parameters**:
   * **tournamentid**: ID of tournament to get data for.
* **Returns:**
    ```javascript
    {
      tournament: [...]
    }
    ```

---

## **get_courts**
Get court information for a unit, a tournament, a specific court or a selection of courts.

Depending on the parameters, the selection of courts returned varies:
- To get all courts registered on a unit, use an HMAC header, or supply a **unitid** parameter.
- To get all courts assigned to a tournament, use the **tournamentid** parameter.
- To get information on one or many specific courts, use the **courtid** parameter.

It is not possible to combine parameters. If using the **tournamentid** or **courtid** parameters, request should be without HMAC header.

* **URL:** /api/court/get_courts
* **Authorization**: none/HMAC
* **Method**: GET or POST
* **Parameters**:
   * **unitid**       (optional): get all courts for the specified unit.
   * **tournamentid** (optional): get all courts for the specified tournament.
   * **courtid**      (optional): get court info for the specified court. This can also be an array, to get info on multiple courts.
   * **addmatchinfo** (optional): append current match data to court object.
   * **addvenueinfo** (optional): append venue data to court object.
* **Returns:**
    ```javascript
    {
      courts: [...] // Array with courts
    }
    ```
---

## **register_device**
Register a device using an activation code. The activation code is generated from the *Device*-tab on the *Tournament* page on the Scoreboard Live web site. The code must be registered within 15 minutes after being generated. 

* **URL:** /api/device/register_device
* **Authorization**: none
* **Method**: POST
* **Parameters**:
   * **activationCode**: Code to use to register device.
* **Returns:**
    ```javascript
    {
      device: {
        activationCode: 'ABC123', // the activation code ued to register device
        clientToken: '...', // token to be used when generating HMAC
        serverToken: '...' // not used atm.
      }
    }
    ```
---

## **check_registration**
This function is used to test if a stored client token is still valid on the server. This is useful to make sure no old keys are being used by the application. If a request to this function returns a **200** response, the key is still valid. If it returns a **403**, the key is no longer valid and should be discarded. 

**Important!** *Any other response than 200 or 403 means the server could not handle the request and the key validity cannot be determined from this!*

* **URL:** /api/device/check_registration
* **Authorization**: HMAC
* **Method**: POST
* **Parameters**: none
* **Returns:**
    ```javascript
    {}
    ```
---

## **create_match**
Creates an 'on-the-fly' match (a match that is not associated with any specific tournament class).

* **URL:** /api/match/create_match
* **Authorization**: HMAC
* **Method**: POST
* **Parameters**:
   * **tournamentid**: (optional) The ID of the tournament to add this match to. If not provided, the best suited currently running tournament will be used.
   * **category**: (required) The match category. Must be one of the following: 
     * **ms**: Men's singles
     * **ws**: Women's singles
     * **md**: Men's doubles
     * **wd**: Women's doubles
     * **xd**: Mixed doubles
   * **description**: (optional) A short description of this match.
   * **umpire**: (optional) Name of the umpire
   * **servicejudge** (optional) Name of the service judge
   * **sequencenumber** (optional) Match number
   * **starttime** (optional) Start time on the format *YYYY-MM-DD HH:MM*. This parameter is optional, but recommended since this match will not show properly in the match list if not set.
   * **team1player1name**: (optional) Player name of first player in first team
   * **team1player1team**: (optional) Team name of first player in first team
   * **team1player2name**: (optional) Player name of second player in first team
   * **team1player2team**: (optional) Team name of second player in first team
   * **team2player1name**: (optional) Player name of first player in second team
   * **team2player1team**: (optional) Team name of first player in second team
   * **team2player2name**: (optional) Player name of second player in second team
   * **team2player2team**: (optional) Team name of second player in second team
* **Returns:**
    ```javascript
    {
      match: {} // New match object
    }
    ```
---

## **assign_match**
Assign a match to a court.

* **URL:** /api/court/assign_match
* **Authorization**: HMAC
* **Method**: POST
* **Parameters**:
   * **courtid** (required): The ID of the court to assign match to.
   * **matchid** (required): The ID of the match to be assigned to a court.
* **Returns:**
    ```javascript
    {}
    ```
---

## **get_matches**
Search for matches.

* **URL:** /api/match/get_matches
* **Authorization**: None
* **Method**: POST
* **Parameters**:
   * **classid**: (optional): ID of class to get matches for. Cannot be used with other parameters.
   * **matchid**: (optional): Get a specific match using its match id. Cannot be used with other parameters.
   * **tournamentid** (optional): The ID of the tournament to find matches in. Use together with *sequencenumber*.
   * **sequencenumber** (optional): The tournament match number to search for. Use together with *tournamentid*.
* **Returns:**
    ```javascript
    {
      match: {} // New match object
    }
    ```
---

# Changelog
#### 2020-06-16
* Now using *System.Text.Json* instead of *Newtonsoft.Json* for response parsing.
* API method *api/match/create_onthefly_match* renamed *api/match/create_match*
