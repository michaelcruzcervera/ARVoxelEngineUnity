using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


    public struct corners<T>
    {
        private T C1 { get; set; }
        private T C2 { get; set; }
        private T C3 { get; set; }
        private T C4 { get; set; }
        private T C5 { get; set; }
        private T C6 { get; set; }
        private T C7 { get; set; }
        private T C8 { get; set; }

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return C1;
                    case 1: return C2;
                    case 2: return C3;
                    case 3: return C4;
                    case 4: return C5;
                    case 5: return C6;
                    case 6: return C7;
                    case 7: return C8;

                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: C1 = value;
                            break;
                    case 1: C2 =value;
                        break;
                    case 2: C3 =value;
                        break;
                    case 3: C4 = value;
                        break;
                    case 4: C5 = value;
                        break;
                    case 5: C6 = value;
                        break;
                    case 6: C7 =value;
                        break;
                    case 7: C8 = value;
                        break;

                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }

    }

    public struct VertexList
    {
        private Vector3 V1 { get; set; }
        private Vector3 V2 { get; set; }
        private Vector3 V3 { get; set; }
        private Vector3 V4 { get; set; }
        private Vector3 V5 { get; set; }
        private Vector3 V6 { get; set; }
        private Vector3 V7 { get; set; }
        private Vector3 V8 { get; set; }
        private Vector3 V9 { get; set; }
        private Vector3 V10 { get; set; }
        private Vector3 V11 { get; set; }
        private Vector3 V12 { get; set; }

        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return V1;
                    case 1: return V2;
                    case 2: return V3;
                    case 3: return V4;
                    case 4: return V5;
                    case 5: return V6;
                    case 6: return V7;
                    case 7: return V8;
                    case 8: return V9;
                    case 9: return V10;
                    case 10: return V11;
                    case 11: return V12;

                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        V1 = value;
                        break;
                    case 1:
                        V2 = value;
                        break;
                    case 2:
                        V3 = value;
                        break;
                    case 3:
                        V4 = value;
                        break;
                    case 4:
                        V5 = value;
                        break;
                    case 5:
                        V6 = value;
                        break;
                    case 6:
                        V7 = value;
                        break;
                    case 7:
                        V8 = value;
                        break;
                    case 8:
                        V9 = value;
                        break;
                    case 9:
                        V10 = value;
                        break;
                    case 10:
                        V11 = value;
                        break;
                    case 11:
                        V12 = value;
                        break;

                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
