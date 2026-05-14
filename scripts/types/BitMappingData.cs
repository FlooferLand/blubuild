using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Godot;
using GodotUtils;

namespace Project;

public record BitMappingData {
    public IReadOnlyDictionary<MappedBit, BitData> BitsToData => bitBitsToData;
    public IReadOnlyDictionary<StringName, MappedBit> AnimToBits => animToBits;

    readonly Dictionary<MappedBit, BitData> bitBitsToData = new();
    readonly Dictionary<StringName, MappedBit> animToBits = new();

    public void Add(MappedBit bit, BitData data) {
        bitBitsToData.Add(bit, data);
        animToBits.Add(data.Anim, bit);
    }
    public void SetFrom(BitMappingData? data) {
        bitBitsToData.Clear();
        animToBits.Clear();
        if (data == null) return;
        foreach (var (key, value) in data.BitsToData)
            Add(key, value);
    }

    // TODO: Switch to a proper parser like https://github.com/sebastienros/parlot

    public Result<string> ToText() {
        var builder = new StringBuilder();
        var sorted = bitBitsToData.OrderBy(entry => entry.Value.Anim);
        foreach (var (bit, data) in sorted) {
            builder.AppendLine($"{bit.Chart}:{bit.Bit} -> {data.Anim}; {data.Flows.In.ToString("0.0")}; {data.Flows.Out.ToString("0.0")}");
        }
        return Result.Ok(builder.ToString());
    }

    public static Result<BitMappingData> FromText(string text) {
        string[] lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var data = new BitMappingData();
        foreach (string line in lines) {
            string[] sections = line.Split("->", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length < 2) return Result.Err("Expected '->'");
            (string bitName, string info) = (sections[0], sections[1]);

            // Bits
            string[] bitArray = bitName.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (bitArray.Length != 2) return Result.Err("Expected bit in the 'chart:id' format");
            (string chart, string bitString) = (bitArray[0], bitArray[1]);
            var bit = new MappedBit(chart, bitString.ToInt());

            // Info
            string[] infoArray = info.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (infoArray.Length < 3) return Result.Err("Expected anim, flowIn, flowOut");
            (string anim, string flowInString, string flowOutString) = (infoArray[0], infoArray[1], infoArray[2]);
            if (!float.TryParse(flowInString, out float flowIn)) return Result.Err("flowIn is not a valid float");
            if (!float.TryParse(flowOutString, out float flowOut)) return Result.Err("flowOut is not a valid float");

            data.Add(bit, new BitData(anim, new Flow(flowIn, flowOut)));
        }
        return Result.Ok(data);
    }
}