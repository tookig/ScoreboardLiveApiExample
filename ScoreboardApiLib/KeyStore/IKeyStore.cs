using System;
namespace ScoreboardLiveApi {
  public interface IKeyStore {
    Device Get(int unitId);
    void Set(Device device);
    void Remove(Device device);
  }
}
