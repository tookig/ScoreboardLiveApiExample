# Scoreboard Live API Reference
This is a short description of the Scoreboard Live API. It does not cover the entire API, but focuses on the parts that can be useful for tournament administrators.

# API basics
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
If an API call fails, the HTTP status code for the return will be 400, 403, 404 or 500 depending on what happened. For 404 and 500, the return value will not be JSON, but a plain HTML standard server response. For the 400 and 403 responses, the return body will be in JSON format, with a description of the error. The 'success'-member is always set to 0, and the errors encountered is found in the 'errors'-array:
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
Most of the API calls requires authorization. Scoreboard Live uses HMAC to authorize HTTP-requests. Each application, or device as they are called internally, must be registered with the Scoreboard Live server to be able to make API-calls that edit data on the server. When the device registers, it will receive an **activation code** (a six character string) and a **client token**. They are both required for authorization. See  [register_device](#register_device).

When an API request requires authorization, the **Authorization**-header must be set. The value should be a string with the first six characters being the **activation code**, and the following being a hex string representation of the HMAC code generated from the **body** of the request being sent, with the **client token** used as a HMAC key. Example of an **Authorization**-header:

```
Authorization: 94T0H86577ef386315bf7c967c40e49a1fb61e05c42dd96e1e2a30bcb78e72d41a4bb5
```

Since the HMAC is generated using only the body of the HTTP-request, the body should always contain some random element; just to add some nonsense-named key with a random generated value.

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
   * (optional) **limit**: max number of tournaments to return.
* **Returns:**
    ```javascript
    {
      tournaments: [...]
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
This function is used to test if a stored client token is still valid on the server. This is useful to make sure no old keys are being used by the application. If a request to this function returns a **200** response, the key is still valid. If it returns a **403**, the key is no longer valid and should be disgarded. 

**Important!** *Any other response means the server could not handle the request and the key validity cannot be determined from this!*

* **URL:** /api/device/register_device
* **Authorization**: none
* **Method**: POST
* **Parameters**:
   * **activationCode**: Code to use to register device.
* **Returns:**
    ```javascript
    {}
    ```
---

