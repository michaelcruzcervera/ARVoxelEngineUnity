using Unity.Mathematics;
using UnityEngine;


    public class LookupTablesDC
    {
    public static readonly float3[] CHILD_MIN_OFFSETS =
{
	// needs to match the vertMap from Dual Contouring impl
	new float3( 0, 0, 0 ),
	new float3( 0, 0, 1 ),
	new float3( 0, 1, 0 ),
	new float3( 0, 1, 1 ),
	new float3( 1, 0, 0 ),
	new float3( 1, 0, 1 ),
	new float3( 1, 1, 0 ),
	new float3( 1, 1, 1 ),
};

    // data from the original DC impl, drives the contouring process

    public static readonly int[][] edgevmap = new int[12][] 
{
	new int[2]{2,4},new int[2]{1,5},new int[2]{2,6},new int[2]{3,7},	// x-axis 
	new int[2]{0,2},new int[2]{1,3},new int[2]{4,6},new int[2]{5,7},	// y-axis
	new int[2]{0,1},new int[2]{2,3},new int[2]{4,5},new int[2]{6,7}		// z-axis
};

    public static readonly int[] edgemask = { 5, 3, 6 };

    public static readonly int[][] vertMap = new int[8][]  
{
	new int[3]{0,0,0},
	new int[3]{0,0,1},
	new int[3]{0,1,0},
	new int[3]{0,1,1},
	new int[3]{1,0,0},
	new int[3]{1,0,1},
	new int[3]{1,1,0},
	new int[3]{1,1,1}
};

    public static readonly int[][] faceMap = new int[6][]
{
    new int[4]{4, 8, 5, 9}, 
    new int[4]{6, 10, 7, 11},
    new int[4]{0, 8, 1, 10},
    new int[4]{2, 9, 3, 11},
    new int[4]{0, 4, 2, 6},
    new int[4]{1, 5, 3, 7}
};

    public static readonly int[][] cellProcFaceMask = new int[12][]
{
    new int[3]{0,4,0},
    new int[3]{1,5,0},
    new int[3]{2,6,0},
    new int[3]{3,7,0},
    new int[3]{0,2,1},
    new int[3]{4,6,1},
    new int[3]{1,3,1},
    new int[3]{5,7,1},
    new int[3]{0,1,2},
    new int[3]{2,3,2},
    new int[3]{4,5,2},
    new int[3]{6,7,2}
};

    public static readonly int[][] cellProcEdgeMask = new int[6][] 
{
    new int[5]{0,1,2,3,0},
    new int[5]{4,5,6,7,0},
    new int[5]{0,4,1,5,1},
    new int[5]{2,6,3,7,1},
    new int[5]{0,2,4,6,2},
    new int[5]{1,3,5,7,2}
};

    public static readonly int[][][] faceProcFaceMask = new int[3][][] 
{
	new int[4][]{ new int[3]{4,0,0}, new int[3]{5,1,0}, new int[3]{6,2,0}, new int[3]{7,3,0} },
	new int[4][]{ new int[3]{2,0,1}, new int[3]{6,4,1}, new int[3]{3,1,1}, new int[3]{7,5,1} },
	new int[4][]{ new int[3]{1,0,2}, new int[3]{3,2,2}, new int[3]{5,4,2}, new int[3]{7,6,2} }
};

    public static readonly int[][][] faceProcEdgeMask = new int[3][][]
{
	new int[4][]{new int[6]{1,4,0,5,1,1},new int[6]{1,6,2,7,3,1},new int[6]{0,4,6,0,2,2},new int[6]{0,5,7,1,3,2}},
	new int[4][]{new int[6]{0,2,3,0,1,0},new int[6]{0,6,7,4,5,0},new int[6]{1,2,0,6,4,2},new int[6]{1,3,1,7,5,2}},
	new int[4][]{new int[6]{1,1,0,3,2,0},new int[6]{1,5,4,7,6,0},new int[6]{0,1,5,0,4,1},new int[6]{0,3,7,2,6,1}}
};

    public static readonly int[][][] edgeProcEdgeMask = new int[3][][]
{
	new int[2][]{new int[5]{3,2,1,0,0},new int[5]{7,6,5,4,0}},
	new int[2][]{new int[5]{5,1,4,0,1},new int[5]{7,3,6,2,1}},
	new int[2][]{new int[5]{6,4,2,0,2},new int[5]{7,5,3,1,2}},
};

    public static readonly int[][] processEdgeMask = new int[3][]
{
    new int[4]{3,2,1,0},new int[4]{7,5,6,4},new int[4]{11,10,9,8}
};
    }
