using System;
using System.Text;
public interface IFixedSizeList<T>
{
    T this[int index] { get; }
    int Length { get; }
}

public struct FixedSizeList_10<T>:IFixedSizeList<T> where T : struct
{
    private T _t0;
    private T _t1;
    private T _t2;
    private T _t3;
    private T _t4;
    private T _t5;
    private T _t6;
    private T _t7;
    private T _t8;
    private T _t9;

    public int Length { get; private set; }
    public int Capacity => 10;

    public void Add(T t)
    {
        Set(Length++, t);
    }

    public void Clear()
    {
        Length = 0;
    }
    
    public bool IsFull()
    {
        return Length == Capacity;
    }

    public bool Contains(T t)
    {
        for (int i = 0; i < Length; i++)
        {
            if (this[i].Equals(t))
            {
                return true;
            }
        }

        return false;
    }

    public T this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return _t0;
                case 1:
                    return _t1;
                case 2:
                    return _t2;
                case 3:
                    return _t3;
                case 4:
                    return _t4;
                case 5:
                    return _t5;
                case 6:
                    return _t6;
                case 7:
                    return _t7;
                case 8:
                    return _t8;
                case 9:
                    return _t9;
                default:
                    throw new IndexOutOfRangeException(index.ToString());
            }
        }
        set
        {
            if (index >= Length-1)
            {
                throw new IndexOutOfRangeException();
            }

            Set(index, value);
        }
    }

    private void Set(int index,T value)
    {
        switch (index)
        {
            case 0:
                _t0 = value;
                break;
            case 1:
                _t1 = value;
                break;
            case 2:
                _t2 = value;
                break;
            case 3:
                _t3 = value;
                break;
            case 4:
                _t4 = value;
                break;
            case 5:
                _t5 = value;
                break;
            case 6:
                _t6 = value;
                break;
            case 7:
                _t7 = value;
                break;
            case 8:
                _t8 = value;
                break;
            case 9:
                _t9 = value;
                break;
            default:
                throw new IndexOutOfRangeException(index.ToString());
        }
    }


    public FixedSizeList_10<T> Fork()
    {
        FixedSizeList_10<T> newChild = new FixedSizeList_10<T>();
        newChild.Length = Length;

        newChild._t0 = _t0;
        newChild._t1 = _t1;
        newChild._t2 = _t2;
        newChild._t3 = _t3;
        newChild._t4 = _t4;

        newChild._t5 = _t5;
        newChild._t6 = _t6;
        newChild._t7 = _t7;
        newChild._t8 = _t8;
        newChild._t9 = _t9;

        return newChild;
    }

    public void Reverse()
    {
        int half = Length / 2;
        for (int i = 0; i < half; i++)
        {
            var tmp = this[i];
            this[i] = this[Length - 1 - i];
            this[Length - 1 - i] = tmp;
        }
    }

    public override string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("(");
        for (int i = 0; i < Length; i++)
        {
            str.Append(this[i]);
            if (i < Length-1)
            {
                str.Append(",");
            }
        }
        str.Append(")");
        return str.ToString();
    }
}

[Serializable]
public struct FixedSizeList_20<T>:IFixedSizeList<T> where T : struct
{
    private T _t0;
    private T _t1;
    private T _t2;
    private T _t3;
    private T _t4;
    private T _t5;
    private T _t6;
    private T _t7;
    private T _t8;
    private T _t9;
    private T _t10;
    private T _t11;
    private T _t12;
    private T _t13;
    private T _t14;
    private T _t15;
    private T _t16;
    private T _t17;
    private T _t18;
    private T _t19;

    public int Length { get; private set; }
    public int Capacity => 20;

    public void Add(T t)
    {
        Set(Length++, t);
    }

    public void Clear()
    {
        Length = 0;
    }

    public bool IsFull()
    {
        return Length == Capacity;
    }

    public T this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return _t0;
                case 1:
                    return _t1;
                case 2:
                    return _t2;
                case 3:
                    return _t3;
                case 4:
                    return _t4;
                case 5:
                    return _t5;
                case 6:
                    return _t6;
                case 7:
                    return _t7;
                case 8:
                    return _t8;
                case 9:
                    return _t9;
                case 10:
                    return _t10;
                case 11:
                    return _t11;
                case 12:
                    return _t12;
                case 13:
                    return _t13;
                case 14:
                    return _t14;
                case 15:
                    return _t15;
                case 16:
                    return _t16;
                case 17:
                    return _t17;
                case 18:
                    return _t18;
                case 19:
                    return _t19;
                default:
                    throw new IndexOutOfRangeException(index.ToString());
            }
        }
        set
        {
            if (index >= Length-1)
            {
                throw new IndexOutOfRangeException();
            }

            Set(index, value);
        }
    }
    
    private void Set(int index,T value)
    {
        switch (index)
        {
            case 0:
                _t0 = value;
                break;
            case 1:
                _t1 = value;
                break;
            case 2:
                _t2 = value;
                break;
            case 3:
                _t3 = value;
                break;
            case 4:
                _t4 = value;
                break;
            case 5:
                _t5 = value;
                break;
            case 6:
                _t6 = value;
                break;
            case 7:
                _t7 = value;
                break;
            case 8:
                _t8 = value;
                break;
            case 9:
                _t9 = value;
                break;
            case 10:
                _t10 = value;
                break;
            case 11:
                _t11 = value;
                break;
            case 12:
                _t12 = value;
                break;
            case 13:
                _t13 = value;
                break;
            case 14:
                _t14 = value;
                break;
            case 15:
                _t15 = value;
                break;
            case 16:
                _t16 = value;
                break;
            case 17:
                _t17 = value;
                break;
            case 18:
                _t18 = value;
                break;
            case 19:
                _t19 = value;
                break;
            default:
                throw new IndexOutOfRangeException(index.ToString());
        }
    }
    
    public void Reverse()
    {
        int half = Length / 2;
        for (int i = 0; i < half; i++)
        {
            var tmp = this[i];
            this[i] = this[Length - 1 - i];
            this[Length - 1 - i] = tmp;
        }
    }

    public FixedSizeList_20<T> Fork()
    {
        FixedSizeList_20<T> newChild = new FixedSizeList_20<T>();
        newChild.Length = Length;

        newChild._t0 = _t0;
        newChild._t1 = _t1;
        newChild._t2 = _t2;
        newChild._t3 = _t3;
        newChild._t4 = _t4;

        newChild._t5 = _t5;
        newChild._t6 = _t6;
        newChild._t7 = _t7;
        newChild._t8 = _t8;
        newChild._t9 = _t9;

        newChild._t10 = _t10;
        newChild._t11 = _t11;
        newChild._t12 = _t12;
        newChild._t13 = _t13;
        newChild._t14 = _t14;

        newChild._t15 = _t15;
        newChild._t16 = _t16;
        newChild._t17 = _t17;
        newChild._t18 = _t18;
        newChild._t19 = _t19;

        return newChild;
    }
    
    public override string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("(");
        for (int i = 0; i < Length; i++)
        {
            str.Append(this[i]);
            if (i < Length-1)
            {
                str.Append(",");
            }
        }
        str.Append(")");
        return str.ToString();
    }
}

public struct FixedSizeList_100<T> : IFixedSizeList<T> where T : struct
{
    private FixedSizeList_10<T> _t0;
    private FixedSizeList_10<T> _t1;
    private FixedSizeList_10<T> _t2;
    private FixedSizeList_10<T> _t3;
    private FixedSizeList_10<T> _t4;
    private FixedSizeList_10<T> _t5;
    private FixedSizeList_10<T> _t6;
    private FixedSizeList_10<T> _t7;
    private FixedSizeList_10<T> _t8;
    private FixedSizeList_10<T> _t9;

    public int Length { get; private set; }
    public int Capacity => 100;

    public void Add(T t)
    {
        int group = Length / 10;
        GetGroup(group).Add(t);
        Length++;
        
    }

    private FixedSizeList_10<T> GetGroup(int groupID)
    {
        switch (groupID)
        {
            case 0:
                return _t0;
            case 1:
                return _t1;
            case 2:
                return _t2;
            case 3:
                return _t3;
            case 4:
                return _t4;
            case 5:
                return _t5;
            case 6:
                return _t6;
            case 7:
                return _t7;
            case 8:
                return _t8;
            case 9:
                return _t9;
            default:
                throw new IndexOutOfRangeException(groupID.ToString());
        }
    }

    public void Clear()
    {
        Length = 0;
    }

    public bool IsFull()
    {
        return Length == Capacity;
    }

    public T this[int index]
    {
        get
        {
            int idInGroup = index % 10;
            int group = Length / 10;
            return GetGroup(group)[idInGroup];
        }
        set
        {
            int idInGroup = index % 10;
            int groupID = Length / 10;
            switch (groupID)
            {
                case 0:
                    _t0[idInGroup] = value;
                    break;
                case 1:
                    _t1[idInGroup] = value;
                    break;
                case 2:
                    _t2[idInGroup] = value;
                    break;
                case 3:
                    _t3[idInGroup] = value;
                    break;
                case 4:
                    _t4[idInGroup] = value;
                    break;
                case 5:
                    _t5[idInGroup] = value;
                    break;
                case 6:
                    _t6[idInGroup] = value;
                    break;
                case 7:
                    _t7[idInGroup] = value;
                    break;
                case 8:
                    _t8[idInGroup] = value;
                    break;
                case 9:
                    _t9[idInGroup] = value;
                    break;
                default:
                    throw new IndexOutOfRangeException(groupID.ToString());
            }
        }
    }

    public FixedSizeList_100<T> Fork()
    {
        FixedSizeList_100<T> newChild = new FixedSizeList_100<T>();
        newChild.Length = Length;
    
        newChild._t0 = _t0;
        newChild._t1 = _t1;
        newChild._t2 = _t2;
        newChild._t3 = _t3;
        newChild._t4 = _t4;
    
        newChild._t5 = _t5;
        newChild._t6 = _t6;
        newChild._t7 = _t7;
        newChild._t8 = _t8;
        newChild._t9 = _t9;
    
        return newChild;
    }

    public void Reverse()
    {
        int half = Length / 2;
        for (int i = 0; i < half; i++)
        {
            var tmp = this[i];
            this[i] = this[Length - 1 - i];
            this[Length - 1 - i] = tmp;
        }
    }

    public override string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("(");
        for (int i = 0; i < Length; i++)
        {
            str.Append(this[i].ToString());
            if (i < Length - 1)
            {
                str.Append(",");
            }
        }

        str.Append(")");
        return str.ToString();
    }

}