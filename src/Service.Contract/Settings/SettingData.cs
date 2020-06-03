﻿using System.Runtime.Serialization;

namespace WebApp.Service.Contract.Settings
{
    [DataContract]
    public class SettingData
    {
        [DataMember(Order = 1)] public string Name { get; set; } = null!;

        [DataMember(Order = 2)] public string? Value { get; set; }

        [DataMember(Order = 3)] public string? DefaultValue { get; set; }

        [DataMember(Order = 4)] public string? MinValue { get; set; }

        [DataMember(Order = 5)] public string? MaxValue { get; set; }
    }
}
