using System;
namespace ScoreboardLiveApi {
  public class Routes {
    public string BaseUrl { get; set; }
      
    public Routes() {
    }

    public Routes(string baseUrl) {
      BaseUrl = baseUrl;
    }

    private string AppendSlash(string url) {
      if (url.EndsWith("/")) {
        return url;
      }
      return url + "/";
    }

    public string GetUnits() {
      return string.Format("{0}api/unit/get_units", AppendSlash(BaseUrl));
    }

    public string RegisterDevice() {
      return string.Format("{0}api/device/register_device", AppendSlash(BaseUrl));
    }

    public string CheckDeviceRegistration() {
      return string.Format("{0}api/device/check_registration", AppendSlash(BaseUrl));
    }
  }
}
