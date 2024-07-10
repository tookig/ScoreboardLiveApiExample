using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardLiveApi {
  /// <summary>
  /// Helper class for error events on the ApiHelper class
  /// </summary>
  public class ApiHelperErrorEventArgs {
    public enum ErrorStage { ConnectionError, ParseError, HttpError }
    
    public ApiHelper Sender { get; private set; }
    public ErrorStage Stage { get; private set; }
    public Exception Exception { get; private set; }

    public ApiHelperErrorEventArgs(ApiHelper sender, ErrorStage stage, Exception exception) {
      Sender = sender;
      Stage = stage;
      Exception = exception;
    }
  }
}
