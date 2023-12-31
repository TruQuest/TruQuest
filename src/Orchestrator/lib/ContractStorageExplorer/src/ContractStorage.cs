using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Numerics;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;

using ContractStorageExplorer.DTO;
using ContractStorageExplorer.SolTypes;

namespace ContractStorageExplorer;

public class ContractStorage
{
    private enum Mode
    {
        Field,
        Mapping,
        Array,
        Value,
    }

    private readonly Contract _contract;
    private Mode _mode;
    private HexBigInteger _slot;
    private int _offset;

    private object? _arrayElementDummyInstance;

    private readonly Dictionary<string, FieldDto> _fields;
    private readonly Dictionary<string, (int SizeBits, Dictionary<string, FieldDto> Fields)>? _structNameToInfo;
    private Dictionary<string, FieldDto>? _structFields;

    internal ContractStorage(Contract contract)
    {
        _contract = contract;
        _mode = Mode.Field;
        _slot = new(0);
        _offset = 0;

        _fields = new();
        foreach (var field in contract._layout.Storage)
        {
            _fields[field.Label] = field;
        }

        foreach (var (_, type) in contract._layout.Types)
        {
            if (type.Encoding == "inplace" && type.Members != null)
            {
                _structNameToInfo = _structNameToInfo ?? new();
                var structName = type.Label.Replace("struct ", string.Empty).Split('.').Last();
                _structNameToInfo[structName] = (SizeBits: type.NumberOfBytes * 8, Fields: new());
                foreach (var member in type.Members)
                {
                    _structNameToInfo[structName].Fields[member.Label] = member;
                }
            }
        }
    }

    private void _ensureIsValueMode()
    {
        if (_mode != Mode.Value)
        {
            throw new Exception("Not value");
        }
    }

    public ContractStorage Field(string fieldName)
    {
        if (_mode != Mode.Field)
        {
            throw new Exception("Not field");
        }

        HexBigInteger fieldSlot;
        if (_structFields == null)
        {
            var field = _fields[fieldName];
            fieldSlot = field.Slot;
            _offset = field.Offset;
        }
        else
        {
            var field = _structFields[fieldName];
            fieldSlot = field.Slot;
            _offset = field.Offset;
        }

        _slot = new HexBigInteger(_slot.Value + fieldSlot.Value);
        _mode = Mode.Value;

        return this;
    }

    public ContractStorage AsMapping()
    {
        _ensureIsValueMode();
        _mode = Mode.Mapping;

        return this;
    }

    public ContractStorage Key<T>(T key) where T : SolType
    {
        if (_mode != Mode.Mapping)
        {
            throw new Exception("Not mapping");
        }

        if (key is SolValueType keyValue)
        {
            if (key is ISolNumber keyNumber)
            {
                if (!keyNumber.IsUnsigned)
                {
                    var hexValue = keyValue.HexValue;
                    var padWith = (hexValue.HexToByteArray()[0] & (byte)8) == 0 ? '0' : 'f';
                    _slot = new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(
                        hexValue.PadLeft(64, padWith),
                        _slot.ToPaddedHexValue()
                    ));
                }
                else
                {
                    _slot = new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(
                        keyValue.HexValue.PadLeft(64, '0'),
                        _slot.ToPaddedHexValue()
                    ));
                }
            }
            else if (key is SolAddress)
            {
                _slot = new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(
                    keyValue.HexValue.PadLeft(64, '0'),
                    _slot.ToPaddedHexValue()
                ));
            }
            else if (key is ISolBytesX)
            {
                _slot = new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(
                    keyValue.HexValue.PadRight(64, '0'),
                    _slot.ToPaddedHexValue()
                ));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else if (key is SolString keyString)
        {
            _slot = new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(
                keyString.HexValue,
                _slot.ToPaddedHexValue()
            ));
        }
        else
        {
            throw new NotImplementedException();
        }

        _offset = 0;
        _mode = Mode.Value;

        return this;
    }

    public ContractStorage AsArrayOf<T>() where T : SolType, new()
    {
        _ensureIsValueMode();
        _arrayElementDummyInstance = Activator.CreateInstance(typeof(T));
        _mode = Mode.Array;

        return this;
    }

    public ContractStorage AsArrayOfStruct(string structName)
    {
        _ensureIsValueMode();

        if (_structNameToInfo == null)
        {
            throw new Exception("No structs defined");
        }

        _arrayElementDummyInstance = new SolStruct(_structNameToInfo[structName].SizeBits);
        _mode = Mode.Array;

        return this;
    }

    public async Task<int> Length()
    {
        if (_mode != Mode.Array)
        {
            throw new Exception("Not array");
        }

        var value = await _contract._web3.Eth.GetStorageAt.SendRequestAsync(_contract._address, _slot);

        return (int)new HexBigInteger(value).Value;
    }

    public ContractStorage Index(int index)
    {
        if (_mode != Mode.Array)
        {
            throw new Exception("Not array");
        }

        var property = _arrayElementDummyInstance!.GetType().GetProperty(
            nameof(SolType.SizeBits),
            BindingFlags.Instance | BindingFlags.Public
        )!;
        var sizeBits = (int)property.GetValue(_arrayElementDummyInstance)!;

        HexBigInteger slotShift;
        if (sizeBits <= 256)
        {
            slotShift = new HexBigInteger(index / (256 / sizeBits));
            _offset = (index % (256 / sizeBits)) * sizeBits;
        }
        else
        {
            slotShift = new HexBigInteger(index * ((sizeBits - 1) / 256 + 1));
            _offset = 0;
        }
        _slot = new HexBigInteger(
            new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(_slot.ToPaddedHexValue())).Value +
            slotShift.Value
        );
        _mode = Mode.Value;

        return this;
    }

    public ContractStorage AsStruct(string structName)
    {
        _ensureIsValueMode();

        if (_structNameToInfo == null)
        {
            throw new Exception("No structs defined");
        }

        _structFields = _structNameToInfo[structName].Fields;
        _mode = Mode.Field;

        return this;
    }

    public async Task<T> GetValue<T>() where T : SolType, new()
    {
        _ensureIsValueMode();

        var value = await _contract._web3.Eth.GetStorageAt.SendRequestAsync(_contract._address, _slot);

        var type = typeof(T);
        if (type.IsSubclassOf(typeof(SolValueType)))
        {
            if (type.IsAssignableTo(typeof(ISolNumber)))
            {
                var property = type.GetProperty(nameof(SolType.SizeBits), BindingFlags.Instance | BindingFlags.Public)!;
                var temp = Activator.CreateInstance(type);
                var sizeBits = (int)property.GetValue(temp)!;
                var maxValueOfTypeBits = new BitArray(Enumerable
                    .Repeat<bool>(false, 256 - sizeBits)
                    .Concat(Enumerable.Repeat<bool>(true, sizeBits))
                    .ToArray()
                );

                var valueBits = new BitArray(value.HexToByteArray());
                valueBits = valueBits.LeftShift(_offset);
                Debug.Assert(valueBits.Count == maxValueOfTypeBits.Count);
                valueBits = valueBits.And(maxValueOfTypeBits);

                var valueBytes = valueBits.ToByteArray();

                property = type.GetProperty(nameof(ISolNumber.IsUnsigned), BindingFlags.Instance | BindingFlags.Public)!;

                return new T
                {
                    ValueObject = new BigInteger(
                        valueBytes.TakeLast(sizeBits / 8).ToArray(),
                        isUnsigned: (bool)property.GetValue(temp)!,
                        isBigEndian: true
                    )
                };
            }
            else if (type.IsAssignableTo(typeof(ISolBytesX)))
            {
                value = value == "0x" ? value + "0000000000000000000000000000000000000000000000000000000000000000" : value;
                var property = type.GetProperty(nameof(SolType.SizeBits), BindingFlags.Instance | BindingFlags.Public)!;
                var temp = Activator.CreateInstance(type);
                var sizeBits = (int)property.GetValue(temp)!;
                var maxValueOfTypeBits = new BitArray(Enumerable
                    .Repeat<bool>(false, 256 - sizeBits)
                    .Concat(Enumerable.Repeat<bool>(true, sizeBits))
                    .ToArray()
                );

                var valueBits = new BitArray(value.HexToByteArray());
                valueBits = valueBits.LeftShift(_offset);
                Debug.Assert(valueBits.Count == maxValueOfTypeBits.Count);
                valueBits = valueBits.And(maxValueOfTypeBits);

                var valueBytes = valueBits.ToByteArray();

                return new T
                {
                    ValueObject = valueBits.ToByteArray().TakeLast(sizeBits / 8).ToArray()
                };
            }
            else if (type == typeof(SolAddress))
            {
                return new T
                {
                    ValueObject = value
                };
            }
            else if (type == typeof(SolBool))
            {
                var valueBits = new BitArray(value.HexToByteArray());
                valueBits = valueBits.LeftShift(_offset);

                var valueBytes = valueBits.ToByteArray();

                return new T
                {
                    ValueObject = valueBytes.Last() == 1
                };
            }
        }
        else if (type == typeof(SolString))
        {
            var valueBytes = value.HexToByteArray();
            if ((valueBytes.Last() & 1) > 0)
            {
                int length = ((int)value.HexToBigInteger(false) - 1) / 2;
                var acc = new List<byte>();
                for (int i = 0; i <= (length - 1) / 32; ++i)
                {
                    var slot = new HexBigInteger(
                        new HexBigInteger(Sha3Keccack.Current.CalculateHashFromHex(_slot.ToPaddedHexValue())).Value +
                        new HexBigInteger(i).Value
                    );

                    var chunk = await _contract._web3.Eth.GetStorageAt.SendRequestAsync(_contract._address, slot);
                    acc.AddRange(chunk.HexToByteArray());
                }

                value = Encoding.UTF8.GetString(acc.Take(length).ToArray());
            }
            else
            {
                int length = valueBytes.Last() / 2;
                value = Encoding.UTF8.GetString(valueBytes.Take(length).ToArray());
            }

            return new T
            {
                ValueObject = value
            };
        }

        throw new NotImplementedException();
    }
}