using Unity.Entities;
using ProjectM.Network;
using System;

namespace ScarletRCON.Data;

public class PlayerData() {
  public Entity UserEntity;
  public User User => UserEntity.Read<User>();
  private string _name = null;
  public string Name {
    get {
      if (string.IsNullOrEmpty(_name)) {
        _name = User.CharacterName.ToString();
      }

      return _name;
    }
  }
  public void SetName(string name) {
    _name = name;
  }
  public Entity CharacterEntity => User.LocalCharacter._Entity;
  public ulong PlatformId => User.PlatformId;
  public bool IsOnline => User.IsConnected;
  public bool IsAdmin => User.IsAdmin;
  public DateTime ConnectedSince => new DateTime(User.TimeLastConnected, DateTimeKind.Utc).ToLocalTime();
}
