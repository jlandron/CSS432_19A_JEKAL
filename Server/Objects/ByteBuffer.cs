using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jekal.Objects
{
    public class ByteBuffer
    {
        private List<byte> _buffer;
        private byte[] _readBuffer;
        private int _readPos;
        private bool _buffUpdated = false;

        public ByteBuffer()
        {
            _buffer = new List<byte>();
            _readPos = 0;
        }
        public int GetReadPosition()
        {
            return _readPos;
        }
        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }
        public int Count()
        {
            return _buffer.Count;
        }
        public int Length()
        {
            return Count() - _readPos;
        }
        public void Clear()
        {
            _buffer.Clear();
            _readPos = 0;
        }
        public void Write(byte input)
        {
            _buffer.Add(input);
            _buffUpdated = true;
        }
        public void Write(byte[] input)
        {
            _buffer.AddRange(input);
            _buffUpdated = true;
        }
        public void Write(short input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input));
            _buffUpdated = true;
        }
        public void Write(int input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input));
            _buffUpdated = true;
        }
        public void Write(long input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input));
            _buffUpdated = true;
        }
        public void Write(float input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input));
            _buffUpdated = true;
        }
        public void Write(bool input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input));
            _buffUpdated = true;
        }
        public void Write(string input)
        {
            _buffer.AddRange(BitConverter.GetBytes(input.Length));
            _buffer.AddRange(Encoding.ASCII.GetBytes(input));
            _buffUpdated = true;
        }
        public byte ReadByte(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                byte value = _readBuffer[_readPos];
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 1;
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read a byte");
            }
        }
        public byte[] ReadBytes(int length, bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                byte[] value = _buffer.GetRange(_readPos, length).ToArray();
                if (Peek)
                {
                    _readPos += length;
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read a byte[]");
            }
        }
        public short ReadShort(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                short value = BitConverter.ToInt16(_readBuffer, _readPos);
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 2; //short is 2 bytes
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read a short");
            }
        }
        public int ReadInt(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                int value = BitConverter.ToInt32(_readBuffer, _readPos);
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 4; //int is 4 bytes
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read an int");
            }
        }
        public long ReadLong(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                long value = BitConverter.ToInt64(_readBuffer, _readPos);
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 8; //long is 8 bytes
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read a long");
            }
        }
        public float ReadFloat(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                float value = BitConverter.ToSingle(_readBuffer, _readPos);
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 4; //float is 4 bytes
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read an float");
            }
        }
        public bool ReadBool(bool Peek = true)
        {
            if (_buffer.Count > _readPos)
            {
                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray(); _buffUpdated = false;
                }
                bool value = BitConverter.ToBoolean(_readBuffer, _readPos);
                if (Peek & _buffer.Count > _readPos)
                {
                    _readPos += 1; //bool is 1 bytes
                }
                return value;
            }
            else
            {
                throw new Exception("You are not trying to read an bool");
            }
        }
        public string ReadString(bool Peek = true)
        {
            try
            {
                int length = ReadInt(true);

                if (_buffUpdated)
                {
                    _readBuffer = _buffer.ToArray();
                    _buffUpdated = false;
                }
                string value = Encoding.ASCII.GetString(_readBuffer, _readPos, length);
                if (Peek & _buffer.Count > _readPos)
                {
                    if (value.Length > 0)
                    {
                        _readPos += length;
                    }
                }
                return value;

            }
            catch (Exception)
            {
                throw new Exception("You are not trying to read an string");
            }

        }
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                if (!disposedValue)
                {
                    _buffer.Clear();
                    _readPos = 0;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
