using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using System.Linq;
using System;
using System.Text.Json.Serialization;

namespace ScarletRCON.Data;

public class PlayerData {
  [JsonIgnore]
  public string Name { get; set; } = default;
  [JsonIgnore]
  public Entity UserEntity { get; set; } = default;
  [JsonIgnore]
  public Entity CharacterEntity { get; set; } = default;
  [JsonIgnore]
  public ulong PlatformID { get; set; } = 0;
  [JsonIgnore]
  public bool IsOnline { get; set; } = false;
  public bool IsAdmin => UserEntity.Read<User>().IsAdmin;
  [JsonIgnore]
  public DateTime ConnectedSince { get; set; } = DateTime.MaxValue;
}
