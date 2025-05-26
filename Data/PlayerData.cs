using Unity.Entities;
using ProjectM.Network;
using System;

namespace ScarletRCON.Data;

public class PlayerData() {
  public Entity UserEntity;
  public User User => UserEntity.Read<User>();
  public string Name => User.CharacterName.ToString();
  public Entity CharacterEntity => User.LocalCharacter._Entity;
  public ulong PlatformId => User.PlatformId;
  public bool IsOnline => User.IsConnected;
  public bool IsAdmin => User.IsAdmin;
  public DateTime ConnectedSince => DateTimeOffset.FromUnixTimeSeconds(User.TimeLastConnected).DateTime;
}
