using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Geometry
{
#region Enums
    [Flags]
    public enum EQuadCorner
    {
        B=1,
        L=2,
        F=4,
        R=8,
    }
    
    [Flags]
    public enum EQuadFacing
    {
        BL=1,
        LF=2,
        FR=4,
        RB=8,
    }
    
    [Flags]
    public enum EQubeCorner
    {
        DB=1,
        DL=2,
        DF=4,
        DR=8,
        
        TB=16,
        TL=32,
        TF=64,
        TR=128,
    }
    
    [Flags]
    public enum ECubeFacing
    {
        B=1,        //BL
        L=2,        //LF
        F=4,        //FR
        R=8,        //RB
        T=16,
        D=32,
    }
#endregion
    
    public interface ITriangle<T> where T : struct
    {
        T this[int _index] { get; }
        T V0 { get;}
        T V1 { get; }
        T V2 { get; }
    }

    [Serializable]
    public struct Triangle<T>: ITriangle<T>,IEquatable<Triangle<T>>,IIterate<T>,IEnumerable<T> where T : struct
    {
        public T v0;
        public T v1;
        public T v2;
        public int Length => 3;
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: Debug.LogError("Invalid Index:" + _index); return v0;
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                }
            }
        }
        
        public Triangle(T _v0, T _v1, T _v2)
        {
            v0 = _v0;
            v1 = _v1;
            v2 = _v2;
        }
        public Triangle((T v0,T v1,T v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2)
        {
        }

        public T V0 => v0;
        public T V1 => v1;
        public T V2 => v2;
    #region Implements
        public bool Equals(Triangle<T> other)
        {
            return v0.Equals(other.v0) && v1.Equals(other.v1) && v2.Equals(other.v2);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = v0.GetHashCode();
                hashCode = (hashCode * 397) ^ v1.GetHashCode();
                hashCode = (hashCode * 397) ^ v2.GetHashCode();
                return hashCode;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Triangle<T> x, Triangle<T> y)
        {
            return x.v0.Equals(y.v0) && x.v1.Equals(y.v1) && x.v2.Equals(y.v2);
        }


        public int GetHashCode(Triangle<T> obj)
        {
            unchecked
            {
                int hashCode = obj.v0.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v1.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v2.GetHashCode();
                return hashCode;
            }
        }
#endregion
    }
    
    public interface IQuad<T>
    {
        T this[int _index] { get; }
        T this[EQuadCorner _corner] { get; }
        T B { get; }
        T L { get; }
        T F { get; }
        T R { get; }
    }

    [Serializable]
    public struct Quad<T> : IQuad<T>,IEquatable<Quad<T>>,IEqualityComparer<Quad<T>>,IIterate<T>,IEnumerable<T>
    {
        public T vB;
        public T vL;
        public T vF;
        public T vR;
        public Quad(T _vB, T _vL, T _vF, T _vR)
        {
            vB = _vB;
            vL = _vL;
            vF = _vF;
            vR = _vR;
        }
        public int Length => 4;
        public T B => vB;
        public T L => vL;
        public T F => vF;
        public T R => vR;

        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Corner:"+_index);
                    case 0: return vB;
                    case 1: return vL;
                    case 2: return vF;
                    case 3: return vR;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Corner:"+_index);
                    case 0: vB = value;  break;
                    case 1: vL = value;  break;
                    case 2: vF = value;  break;
                    case 3: vR = value;  break;
                }
            }
        }
        public T this[EQuadCorner _corner]
        {
            get
            {
                switch (_corner)
                {
                    default: throw new Exception("Invalid Corner:"+_corner);
                    case EQuadCorner.B: return vB;
                    case EQuadCorner.L: return vL;
                    case EQuadCorner.F: return vF;
                    case EQuadCorner.R: return vR;
                }
            }
            set
            {
                switch (_corner)
                {
                    default: throw new Exception("Invalid Corner:"+_corner);
                    case EQuadCorner.B: vB = value;  break;
                    case EQuadCorner.L: vL = value;  break;
                    case EQuadCorner.F: vF = value;  break;
                    case EQuadCorner.R: vR = value;  break;
                }
            }
        }

        public static Quad<T> Convert<Y>(Quad<Y> _srcQuad, Func<Y, T> _convert) => new Quad<T>(_convert(_srcQuad.vB), _convert(_srcQuad.vL), _convert(_srcQuad.vF), _convert(_srcQuad.vR));
        public static Quad<T> Convert<Y>(Quad<Y> _srcQuad, Func<int,Y, T> _convert) => new Quad<T>(_convert(0, _srcQuad.vB), _convert(1, _srcQuad.vL), _convert(2, _srcQuad.vF),_convert(3, _srcQuad.vR));
        #region Implements
        public IEnumerator<T> GetEnumerator()
        {
            yield return vB;
            yield return vL;
            yield return vF;
            yield return vR;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Quad<T> other)
        {
            return vB.Equals(other.vB) && vL.Equals(other.vL) && vF.Equals(other.vF) && vR.Equals(other.vR);
        }

        public override bool Equals(object obj)
        {
            return obj is Quad<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = vB.GetHashCode();
                hashCode = (hashCode * 397) ^ vL.GetHashCode();
                hashCode = (hashCode * 397) ^ vF.GetHashCode();
                hashCode = (hashCode * 397) ^ vR.GetHashCode();
                return hashCode;
            }
        }
        
        public bool Equals(Quad<T> x, Quad<T> y)
        {
            return x.vB.Equals(y.vB) && x.vL.Equals(y.vL) && x.vF.Equals(y.vF) && x.vR.Equals(y.vR);
        }

        public int GetHashCode(Quad<T> obj)
        {
            unchecked
            {
                int hashCode = obj.vB.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vL.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vF.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.vR.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }

    [Serializable]
    public struct CubeSides<T>:IEnumerable<T> 
    {
        public T fBL;
        public T fLF;
        public T fFR;
        public T fRB;
        public T fT;
        public T fD;

        public CubeSides(T _fBL, T _fLF, T _fFR, T _fRB,T _fT,T _fD)
        {
            fBL = _fBL;
            fLF = _fLF;
            fFR = _fFR;
            fRB = _fRB;
            fT = _fT;
            fD = _fD;
        }

        public static CubeSides<T> Create<Y>(CubeSides<Y> _src, Func<Y, T> _convert)  =>
            new CubeSides<T>(_convert(_src.fBL),_convert(_src.fLF),_convert(_src.fFR),_convert(_src.fRB),_convert(_src.fT),_convert(_src.fD));
        
        public T this[ECubeFacing _facing]
        {
            get => this[_facing.FacingToIndex()];
            set => this[_facing.FacingToIndex()] = value;
        }

        public T this[EQuadFacing _quadFacing]
        {
            get
            {
                switch (_quadFacing)
                {
                    default: throw new InvalidEnumArgumentException();
                    case EQuadFacing.BL: return fBL;
                    case EQuadFacing.FR: return fFR;
                    case EQuadFacing.LF: return fLF;
                    case EQuadFacing.RB: return fRB;
                }
            }
        }
        
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Facing:"+_index);
                    case 0: return fBL;
                    case 1: return fLF;
                    case 2: return fFR;
                    case 3: return fRB;
                    case 4: return fT;
                    case 5: return fD;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new Exception("Invalid Facing:"+_index);
                    case 0: fBL=value; break;
                    case 1: fLF=value; break;
                    case 2: fFR=value; break;
                    case 3: fRB=value; break;
                    case 4: fT=value; break;
                    case 5: fD=value; break;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return fBL;
            yield return fLF;
            yield return fFR;
            yield return fRB;
            yield return fT;
            yield return fD;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public interface IQube<T>
    {
        T this[int _index] { get; }
        public T DB { get; }
        public T DL { get; }
        public T DF { get; }
        public T DR { get; }
        public T TB { get; }
        public T TL { get; }
        public T TF { get; }
        public T TR { get; }
    }

    [Serializable]
    public struct Qube<T> : IQube<T>, IEquatable<Qube<T>>,IEqualityComparer<Qube<T>>,IIterate<T>,IEnumerable<T>
    {
        public T vDB;
        public T vDL;
        public T vDF;
        public T vDR;
        public T vTB;
        public T vTL;
        public T vTF;
        public T vTR;
        public Qube(T _value)  { vDB = _value;  vDL = _value; vDF = _value; vDR = _value; vTB = _value; vTL = _value; vTF = _value; vTR = _value; }
        public Qube( T _vertDB, T _vertDL, T _vertDF, T _vertDR, 
            T _vertTB, T _vertTL, T _vertTF, T _vertTR)
        {
            vDB = _vertDB;
            vDL = _vertDL;
            vDF = _vertDF;
            vDR = _vertDR;
            vTB = _vertTB;
            vTL = _vertTL;
            vTF = _vertTF;
            vTR = _vertTR;
        }

        public Qube(Quad<T> _downQuad,Quad<T> _topQuad):this(
            _downQuad.vB,_downQuad.vL,_downQuad.vF,_downQuad.vR,
            _topQuad.vB, _topQuad.vL,_topQuad.vF,_topQuad.vR) 
        {
        }

        public T this[EQubeCorner _corner]
        {
            get => this[_corner.CornerToIndex()];
            set => this[_corner.CornerToIndex()]=value;
        }
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: return vDB;
                    case 1: return vDL;
                    case 2: return vDF;
                    case 3: return vDR;
                    case 4: return vTB;
                    case 5: return vTL;
                    case 6: return vTF;
                    case 7: return vTR;
                }
            }
            set
            {
                switch (_index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: vDB = value; break;
                    case 1: vDL = value; break;
                    case 2: vDF = value; break;
                    case 3: vDR = value; break;
                    case 4: vTB = value; break;
                    case 5: vTL = value; break;
                    case 6: vTF = value; break;
                    case 7: vTR = value; break;
                }
            }
        }

        public T GetElement(int _index) => this[_index];
        public int Length => 8;
        public T DB => vDB;
        public T DL => vDL;
        public T DF => vDF;
        public T DR => vDR;
        public T TB => vTB;
        public T TL => vTL;
        public T TF => vTF;
        public T TR => vTR;
        
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            yield return vDB;
            yield return vDL;
            yield return vDF;
            yield return vDR;
            yield return vTB;
            yield return vTL;
            yield return vTF;
            yield return vTR;
        }

        public IEnumerable<T> IterateTop()
        {
            yield return vTB;
            yield return vTL;
            yield return vTF;
            yield return vTR;
        }
        
        public IEnumerable<T> IterateDown()
        {
            yield return vDB;
            yield return vDL;
            yield return vDF;
            yield return vDR;
        }
        
        static readonly EqualityComparer<T> kComparer= EqualityComparer<T>.Default;
        public bool Equals(Qube<T> other)
        {
            return kComparer.Equals(vDB,other.vDB) &&
                   kComparer.Equals(vDL,other.vDL) &&
                   kComparer.Equals(vDF,other.vDF) &&
                   kComparer.Equals(vDR,other.vDR) && 
                   kComparer.Equals(vTB,other.vTB) &&
                   kComparer.Equals(vTL,other.vTL) &&
                   kComparer.Equals(vTF,other.vTF) &&
                   kComparer.Equals(vTR,other.vTR);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = vDB.GetHashCode();
                hashCode = (hashCode * 397) ^ vDL.GetHashCode();
                hashCode = (hashCode * 397) ^ vDF.GetHashCode();
                hashCode = (hashCode * 397) ^ vDR.GetHashCode();
                hashCode = (hashCode * 397) ^ vTB.GetHashCode();
                hashCode = (hashCode * 397) ^ vTL.GetHashCode();
                hashCode = (hashCode * 397) ^ vTF.GetHashCode();
                hashCode = (hashCode * 397) ^ vTR.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()=> $"{vDB} {vDL} {vDF} {vDR} / {vTB} {vTL} {vTF} {vTR}";
        
        public static Qube<T> Convert<Y>(Qube<Y> _srcQuad, Func<Y, T> _convert)
        {
            return new Qube<T>(_convert(_srcQuad.vDB), _convert(_srcQuad.vDL), _convert(_srcQuad.vDF), _convert(_srcQuad.vDR),
                _convert(_srcQuad.vTB), _convert(_srcQuad.vTL), _convert(_srcQuad.vTF), _convert(_srcQuad.vTR));
        }
        public static Qube<T> Convert<Y>(Qube<Y> _srcQuad, Func<int,Y, T> _convert)
        {
            return new Qube<T>(_convert(0,_srcQuad.vDB), _convert(1,_srcQuad.vDL), _convert(2,_srcQuad.vDF), _convert(3,_srcQuad.vDR),
                _convert(4,_srcQuad.vTB), _convert(5,_srcQuad.vTL), _convert(6,_srcQuad.vTF), _convert(7,_srcQuad.vTR));
        }

        public bool Equals(Qube<T> x, Qube<T> y)
        {
            return EqualityComparer<T>.Default.Equals(x.vDB, y.vDB) && EqualityComparer<T>.Default.Equals(x.vDL, y.vDL) && EqualityComparer<T>.Default.Equals(x.vDF, y.vDF) && EqualityComparer<T>.Default.Equals(x.vDR, y.vDR) && EqualityComparer<T>.Default.Equals(x.vTB, y.vTB) && EqualityComparer<T>.Default.Equals(x.vTL, y.vTL) && EqualityComparer<T>.Default.Equals(x.vTF, y.vTF) && EqualityComparer<T>.Default.Equals(x.vTR, y.vTR);
        }

        public int GetHashCode(Qube<T> obj)
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(obj.vDB);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDL);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDF);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vDR);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTB);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTL);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTF);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(obj.vTR);
                return hashCode;
            }
        }
    }
    
}