using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class NaiveDualContouring
{

    public static int MATERIAL_AIR = 0;
    public static int MATERIAL_SOLID = 1;

    public static float QEF_ERROR = 1e-6f;
    public static int QEF_SWEEPS = 4;


    public static OctreeNode SimplifyOctree(OctreeNode node, float threshold)
    {
        if (node == null)
        {
            return null;
        }

        if (node.Type != OctreeNodeType.Node_Internal)
        {
            // can't simplify!
            return node;
        }

        QefSolver qef = new QefSolver();
        int[] signs = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
        int midsign = -1;
        int edgeCount = 0;
        bool isCollapsible = true;

        for (int i = 0; i < 8; i++)
        {
            node.children[i] = SimplifyOctree(node.children[i], threshold);

            if (node.children[i] != null)
            {
                OctreeNode child = node.children[i];

                if (child.Type == OctreeNodeType.Node_Internal)
                {
                    isCollapsible = false;
                }
                else
                {
                    qef.add(child.drawInfo.qef);

                    midsign = (child.drawInfo.corners >> (7 - i)) & 1;
                    signs[i] = (child.drawInfo.corners >> i) & 1;

                    edgeCount++;
                }
            }
        }

        if (!isCollapsible)
        {
            // at least one child is an internal node, can't collapse
            return node;
        }

        float3 qefPosition = float3.zero;
        qef.solve(qefPosition, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);
        float error = qef.getError();

        // convert to SDF vec3 for ease of use
        float3 position = new float3(qefPosition.x, qefPosition.y, qefPosition.z);

        // at this point the masspoint will actually be a sum, so divide to make it the average
        if (error > threshold)
        {
            // this collapse breaches the threshold
            return node;
        }

        if (position.x < node.min.x || position.x > (node.min.x + node.size) ||
            position.y < node.min.y || position.y > (node.min.y + node.size) ||
            position.z < node.min.z || position.z > (node.min.z + node.size))
        {
            position = qef.getMassPoint();
        }

        // change the node from an internal node to a 'psuedo leaf' node
        OctreeDrawInfo drawInfo = new OctreeDrawInfo();
        drawInfo.corners = 0;
        drawInfo.index = -1;

        for (int i = 0; i < 8; i++)
        {
            if (signs[i] == -1)
            {
                // Undetermined, use centre sign instead
                drawInfo.corners |= (midsign << i);
            }
            else
            {
                drawInfo.corners |= (signs[i] << i);
            }
        }

        drawInfo.averageNormal = float3.zero;
        for (int i = 0; i < 8; i++)
        {
            if (node.children[i] != null)
            {
                OctreeNode child = node.children[i];
                if (child.Type == OctreeNodeType.Node_Psuedo ||
                    child.Type == OctreeNodeType.Node_Leaf)
                {
                    drawInfo.averageNormal += child.drawInfo.averageNormal;
                }
            }
        }

        drawInfo.averageNormal = drawInfo.averageNormal.normalized;
        drawInfo.position = position;
        drawInfo.qef = qef.getData();

        for (int i = 0; i < 8; i++)
        {
            DestroyOctree(node.children[i]);
            node.children[i] = null;
        }

        node.Type = OctreeNodeType.Node_Psuedo;
        node.drawInfo = drawInfo;

        return node;
    }

    public static void GenerateVertexIndices(OctreeNode node, List<MeshVertex> vertexBuffer)
    {
        if (node == null)
        {
            return;
        }

        if (node.Type != OctreeNodeType.Node_Leaf)
        {
            for (int i = 0; i < 8; i++)
            {
                GenerateVertexIndices(node.children[i], vertexBuffer);
            }
        }

        if (node.Type != OctreeNodeType.Node_Internal)
        {
            node.drawInfo.index = vertexBuffer.Count;

            vertexBuffer.Add(new MeshVertex(node.drawInfo.position, node.drawInfo.averageNormal));
        }
    }

    public static void ContourProcessEdge(OctreeNode[] node, int dir, List<int> indexBuffer)
    {
        int minSize = 1000000;		// arbitrary big number
        int minIndex = 0;
        int[] indices = new int[4] { -1, -1, -1, -1 };
        bool flip = false;
        bool[] signChange = new bool[4] { false, false, false, false };

        for (int i = 0; i < 4; i++)
        {
            int edge = LookupTablesDC.processEdgeMask[dir][i];
            int c1 = LookupTablesDC.edgevmap[edge][0];
            int c2 = LookupTablesDC.edgevmap[edge][1];

            int m1 = (node[i].drawInfo.corners >> c1) & 1;
            int m2 = (node[i].drawInfo.corners >> c2) & 1;

            if (node[i].size < minSize)
            {
                minSize = node[i].size;
                minIndex = i;
                flip = m1 != MATERIAL_AIR;
            }

            indices[i] = node[i].drawInfo.index;

            signChange[i] =
                (m1 == MATERIAL_AIR && m2 != MATERIAL_AIR) ||
                (m1 != MATERIAL_AIR && m2 == MATERIAL_AIR);
        }

        if (signChange[minIndex])
        {
            if (!flip)
            {
                indexBuffer.Add(indices[0]);
                indexBuffer.Add(indices[1]);
                indexBuffer.Add(indices[3]);

                indexBuffer.Add(indices[0]);
                indexBuffer.Add(indices[3]);
                indexBuffer.Add(indices[2]);
            }
            else
            {
                indexBuffer.Add(indices[0]);
                indexBuffer.Add(indices[3]);
                indexBuffer.Add(indices[1]);

                indexBuffer.Add(indices[0]);
                indexBuffer.Add(indices[2]);
                indexBuffer.Add(indices[3]);
            }

        }
    }

    public static void ContourEdgeProc(OctreeNode[] node, int dir, List<int> indexBuffer)
    {
        if (node[0] == null || node[1] == null || node[2] == null || node[3] == null)
        {
            return;
        }

        if (node[0].Type != OctreeNodeType.Node_Internal &&
            node[1].Type != OctreeNodeType.Node_Internal &&
            node[2].Type != OctreeNodeType.Node_Internal &&
            node[3].Type != OctreeNodeType.Node_Internal)
        {
            ContourProcessEdge(node, dir, indexBuffer);
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                OctreeNode[] edgeNodes = new OctreeNode[4];
                int[] c = new int[4]
			{
                LookupTablesDC.edgeProcEdgeMask[dir][i][0],
                LookupTablesDC.edgeProcEdgeMask[dir][i][1],
                LookupTablesDC.edgeProcEdgeMask[dir][i][2],
                LookupTablesDC.edgeProcEdgeMask[dir][i][3],
			};

                for (int j = 0; j < 4; j++)
                {
                    if (node[j].Type == OctreeNodeType.Node_Leaf || node[j].Type == OctreeNodeType.Node_Psuedo)
                    {
                        edgeNodes[j] = node[j];
                    }
                    else
                    {
                        edgeNodes[j] = node[j].children[c[j]];
                    }
                }

                ContourEdgeProc(edgeNodes, LookupTablesDC.edgeProcEdgeMask[dir][i][4], indexBuffer);
            }
        }
    }

    public static void ContourFaceProc(OctreeNode[] node, int dir, List<int> indexBuffer)
    {
        if (node[0] == null || node[1] == null)
        {
            return;
        }

        if (node[0].Type == OctreeNodeType.Node_Internal ||
            node[1].Type == OctreeNodeType.Node_Internal)
        {
            for (int i = 0; i < 4; i++)
            {
                OctreeNode[] faceNodes = new OctreeNode[2];
                int[] c = new int[2] 
			{
				LookupTablesDC.faceProcFaceMask[dir][i][0], 
				LookupTablesDC.faceProcFaceMask[dir][i][1], 
			};

                for (int j = 0; j < 2; j++)
                {
                    if (node[j].Type != OctreeNodeType.Node_Internal)
                    {
                        faceNodes[j] = node[j];
                    }
                    else
                    {
                        faceNodes[j] = node[j].children[c[j]];
                    }
                }

                ContourFaceProc(faceNodes, LookupTablesDC.faceProcFaceMask[dir][i][2], indexBuffer);
            }

            int[][] orders = new int[2][] 
		{
			new int[4]{ 0, 0, 1, 1 },
			new int[4]{ 0, 1, 0, 1 },
		};

            for (int i = 0; i < 4; i++)
            {
                OctreeNode[] edgeNodes = new OctreeNode[4];
                int[] c = new int[4]
			{
				LookupTablesDC.faceProcEdgeMask[dir][i][1],
				LookupTablesDC.faceProcEdgeMask[dir][i][2],
				LookupTablesDC.faceProcEdgeMask[dir][i][3],
				LookupTablesDC.faceProcEdgeMask[dir][i][4],
			};

                int[] order = orders[LookupTablesDC.faceProcEdgeMask[dir][i][0]];
                for (int j = 0; j < 4; j++)
                {
                    if (node[order[j]].Type == OctreeNodeType.Node_Leaf ||
                        node[order[j]].Type == OctreeNodeType.Node_Psuedo)
                    {
                        edgeNodes[j] = node[order[j]];
                    }
                    else
                    {
                        edgeNodes[j] = node[order[j]].children[c[j]];
                    }
                }

                ContourEdgeProc(edgeNodes, LookupTablesDC.faceProcEdgeMask[dir][i][5], indexBuffer);
            }
        }
    }

    public static void ContourCellProc(OctreeNode node, List<int> indexBuffer)
    {
        if (node == null)
        {
            return;
        }

        if (node.Type == OctreeNodeType.Node_Internal)
        {
            for (int i = 0; i < 8; i++)
            {
                ContourCellProc(node.children[i], indexBuffer);
            }

            for (int i = 0; i < 12; i++)
            {
                OctreeNode[] faceNodes = new OctreeNode[2];
                int[] c = { LookupTablesDC.cellProcFaceMask[i][0], LookupTablesDC.cellProcFaceMask[i][1] };

                faceNodes[0] = node.children[c[0]];
                faceNodes[1] = node.children[c[1]];

                ContourFaceProc(faceNodes, LookupTablesDC.cellProcFaceMask[i][2], indexBuffer);
            }

            for (int i = 0; i < 6; i++)
            {
                OctreeNode[] edgeNodes = new OctreeNode[4];
                int[] c = new int[4]
			{
				LookupTablesDC.cellProcEdgeMask[i][0],
				LookupTablesDC.cellProcEdgeMask[i][1],
				LookupTablesDC.cellProcEdgeMask[i][2],
				LookupTablesDC.cellProcEdgeMask[i][3],
			};

                for (int j = 0; j < 4; j++)
                {
                    edgeNodes[j] = node.children[c[j]];
                }

                ContourEdgeProc(edgeNodes, LookupTablesDC.cellProcEdgeMask[i][4], indexBuffer);
            }
        }
    }

    public static float3 ApproximateZeroCrossingPosition(float3 p0, float3 p1)
    {
        // approximate the zero crossing by finding the min value along the edge
        float minValue = 100000f;
        float t = 0f;
        float currentT = 0f;
        const int steps = 8;
        const float increment = 1f / (float)steps;
        while (currentT <= 1.0f)
        {
            float3 p = p0 + ((p1 - p0) * currentT);
            float density = Mathf.Abs(SDF.Density_Func(p));
            if (density < minValue)
            {
                minValue = density;
                t = currentT;
            }

            currentT += increment;
        }

        return p0 + ((p1 - p0) * t);
    }

    public static float3 CalculateSurfaceNormal(float3 p)
    {
        float H = 0.001f;
        float dx = SDF.Density_Func(p + new float3(H, 0.0f, 0.0f)) - SDF.Density_Func(p - new float3(H, 0.0f, 0.0f));
        float dy = SDF.Density_Func(p + new float3(0.0f, H, 0.0f)) - SDF.Density_Func(p - new float3(0.0f, H, 0.0f));
        float dz = SDF.Density_Func(p + new float3(0.0f, 0.0f, H)) - SDF.Density_Func(p - new float3(0.0f, 0.0f, H));

        return math.normalize(new float3(dx, dy, dz));
    }

    public static OctreeNode ConstructLeaf(OctreeNode leaf)
    {
        if (leaf == null || leaf.size != 1)
        {
            return null;
        }

        int corners = 0;
        for (int i = 0; i < 8; i++)
        {
            float3 cornerPos = leaf.min + LookupTablesDC.CHILD_MIN_OFFSETS[i];
            float density = SDF.Density_Func(cornerPos);
            int material = density < 0.0f ? MATERIAL_SOLID : MATERIAL_AIR;
            corners |= (material << i);
        }

        if (corners == 0 || corners == 255)
        {
            // voxel is full inside or outside the volume
            //delete leaf
            //setting as null isn't required by the GC in C#... but its in the original, so why not!
            leaf = null;
            return null;
        }

        // otherwise the voxel contains the surface, so find the edge intersections
        const int MAX_CROSSINGS = 6;
        int edgeCount = 0;
        float3 averageNormal = float3.zero;
        QefSolver qef = new QefSolver();

        for (int i = 0; i < 12 && edgeCount < MAX_CROSSINGS; i++)
        {
            int c1 = LookupTablesDC.edgevmap[i][0];
            int c2 = LookupTablesDC.edgevmap[i][1];

            int m1 = (corners >> c1) & 1;
            int m2 = (corners >> c2) & 1;

            if ((m1 == MATERIAL_AIR && m2 == MATERIAL_AIR) || (m1 == MATERIAL_SOLID && m2 == MATERIAL_SOLID))
            {
                // no zero crossing on this edge
                continue;
            }

            float3 p1 = leaf.min + LookupTablesDC.CHILD_MIN_OFFSETS[c1];
            float3 p2 = leaf.min + LookupTablesDC.CHILD_MIN_OFFSETS[c2];
            float3 p = ApproximateZeroCrossingPosition(p1, p2);
            float3 n = CalculateSurfaceNormal(p);
            qef.add(p.x, p.y, p.z, n.x, n.y, n.z);

            averageNormal += n;

            edgeCount++;
        }

        float3 qefPosition = float3.zero;
        qef.solve(qefPosition, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);

        OctreeDrawInfo drawInfo = new OctreeDrawInfo();
        drawInfo.corners = 0;
        drawInfo.index = -1;
        drawInfo.position = new float3(qefPosition.x, qefPosition.y, qefPosition.z);
        drawInfo.qef = qef.getData();

        float3 min = leaf.min;
        float3 max = new float3(leaf.min.x + leaf.size, leaf.min.y + leaf.size, leaf.min.z + leaf.size);
        if (drawInfo.position.x < min.x || drawInfo.position.x > max.x ||
            drawInfo.position.y < min.y || drawInfo.position.y > max.y ||
            drawInfo.position.z < min.z || drawInfo.position.z > max.z)
        {
            drawInfo.position = qef.getMassPoint();
        }

        drawInfo.averageNormal = math.normalize(averageNormal / (float)edgeCount);
        drawInfo.corners = corners;

        leaf.Type = OctreeNodeType.Node_Leaf;
        leaf.drawInfo = drawInfo;

        return leaf;
    }

    public static OctreeNode ConstructOctreeNodes(OctreeNode node)
    {
        if (node == null)
        {
            return null;
        }

        if (node.size == 1)
        {
            return ConstructLeaf(node);
        }

        int childSize = node.size / 2;
        bool hasChildren = false;

        for (int i = 0; i < 8; i++)
        {
            OctreeNode child = new OctreeNode();
            child.size = childSize;
            child.min = node.min + (LookupTablesDC.CHILD_MIN_OFFSETS[i] * childSize);
            child.Type = OctreeNodeType.Node_Internal;

            node.children[i] = ConstructOctreeNodes(child);
            hasChildren |= (node.children[i] != null);
        }

        if (!hasChildren)
        {
            //delete leaf
            //setting as null isn't required by the GC in C#... but its in the original, so why not!
            node = null;
            return null;
        }

        return node;
    }

    public static OctreeNode BuildOctree(float3 min, int size, float threshold)
    {
        Debug.Log(string.Format("Building Octree at {0}, with size of {1} and threshold of {2}", min, size, threshold));

        OctreeNode root = new OctreeNode();
        root.min = min;
        root.size = size;
        root.Type = OctreeNodeType.Node_Internal;

        root = ConstructOctreeNodes(root);
        root = SimplifyOctree(root, threshold);

        return root;
    }

    public static void GenerateMeshFromOctree(OctreeNode node, List<MeshVertex> vertexBuffer, List<int> indexBuffer)
    {
        if (node == null)
        {
            return;
        }


        GenerateVertexIndices(node, vertexBuffer);
        ContourCellProc(node, indexBuffer);
        
    }

    public static void DestroyOctree(OctreeNode node)
    {
        if (node == null)
        {
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            DestroyOctree(node.children[i]);
        }

        node = null;
    }

}