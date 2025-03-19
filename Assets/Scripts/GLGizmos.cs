using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using EX;

//[ExecuteInEditMode]
public class GLGizmos : MonoBehaviour
{
    private Material GLmat;
    private static Color? color = Color.white;
    private static List<Action> drawActions = new List<Action>();
    public Main main;

    public enum ArcCloseType { None, Flat, Center, Edge }

    private void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        CreateGLMaterial();
    }

    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        DestroyGLMaterial();
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPreRender();
    }

    private void OnPreRender()
    {
        GL.wireframe = false;
        float aspectMatch = ((16f / 9f) / ((float)Camera.main.scaledPixelWidth / (float)Camera.main.scaledPixelHeight));
        main.SetBounds(Mathf.Min(Mathf.Pow(aspectMatch, .25f) * 4.5f, 4.5f), .25f * (1f / Mathf.Pow(aspectMatch, 1.25f)));
    }

    private void OnPostRender()
    {
        GLmat.SetPass(0);
        GL.PushMatrix();

        foreach (var draw in drawActions)
            draw.Invoke();

        GL.PopMatrix();
        //drawActions.Clear();

        GL.wireframe = false;
    }

    public static void ClearDrawActions() => drawActions.Clear();

    /// <summary>
    /// Sets the global color parameter of GLGizmos
    /// </summary>
    public static void SetColor(Color colorSet)
        => drawActions.Add(() => InternalSetColor(colorSet));

    private static void InternalSetColor(Color colorSet) => color = colorSet;



    /// <summary>
    /// Draws an open box at 'position' with 'size'
    /// </summary>
    public static void DrawOpenBox(Vector2 position, Vector2 size, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawBox(position, size, false, colorSetting));

    /// <summary>
    /// Draws a solid box at 'position' with 'size'
    /// </summary>
    public static void DrawSolidBox(Vector2 position, Vector2 size, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawBox(position, size, true, colorSetting));

    private static void InternalDrawBox(Vector2 position, Vector2 size, bool solid, Color? colorSetting = null)
    {
        GL.wireframe = !solid;
        GL.Begin(GL.wireframe ? GL.LINES : GL.QUADS);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        int signX = -1, signY = -1;
        bool flipY = true;
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(position + new Vector2(signX * size.x / 2, signY * size.y / 2));

            if (flipY)
                signY *= -1;
            else
                signX *= -1;

            if (GL.wireframe)
                GL.Vertex(position + new Vector2(signX * size.x / 2, signY * size.y / 2));

            flipY = !flipY;
        }

        GL.End();
    }



    /// <summary>
    /// Draws multiple open boxes at 'positions' with 'size', optional cycle through 'colors' list
    /// </summary>
    public static void DrawOpenBoxes(List<Vector2> positions, Vector2 size, List<Color> colors = null)
        => drawActions.Add(() => InternalDrawBoxes(positions, size, false, colors));

    /// <summary>
    /// Draws multiple solid boxes at 'positions' with 'size', optional cycle through 'colors' list
    /// </summary>
    public static void DrawSolidBoxes(List<Vector2> positions, Vector2 size, List<Color> colors = null)
        => drawActions.Add(() => InternalDrawBoxes(positions, size, true, colors));

    private static void InternalDrawBoxes(List<Vector2> positions, Vector2 size, bool solid, List<Color> colors = null)
    {
        if (positions == null || positions.Count == 0)
            return;

        GL.wireframe = !solid;
        bool noColor = colors == null || colors.Count == 0;
        int i = 0;
        foreach (Vector2 position in positions)
        {
            InternalDrawBox(position, size, solid, noColor ? color.Value : colors[i % colors.Count]);

            i++;
        }
    }




    /// <summary>
    /// Draws the edge radius of a box defined by the box size and edge radius
    /// </summary>
    public static void DrawBoxEdgeRadius(Vector2 position, Vector2 size, float edgeRadius, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawBoxEdgeRadius(position, size, edgeRadius, colorSetting));

    private static void InternalDrawBoxEdgeRadius(Vector2 position, Vector2 size, float edgeRadius, Color? colorSetting = null)
    {
        GL.wireframe = false;
        GL.Begin(GL.LINE_STRIP);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        int num = 8;
        Vector2 halfSize = new Vector2(size.x / 2, size.y / 2);
        Vector2 topRight = position + halfSize.MultiplyEach(1, 1);
        Vector2 topLeft = position + halfSize.MultiplyEach(-1, 1);
        Vector2 bottomLeft = position + halfSize.MultiplyEach(-1, -1);
        Vector2 bottomRight = position + halfSize.MultiplyEach(1, -1);

        Vector2 vectorTopRight = topRight + Vector2.up * edgeRadius;
        Vector2 vectorTopLeft = topLeft + Vector2.up * edgeRadius;

        Vector2 vectorLeftUp = topLeft + Vector2.left * edgeRadius;
        Vector2 vectorLeftDown = bottomLeft + Vector2.left * edgeRadius;

        Vector2 vectorBottomLeft = bottomLeft + Vector2.down * edgeRadius;
        Vector2 vectorBottomRight = bottomRight + Vector2.down * edgeRadius;

        Vector2 vectorRightDown = bottomRight + Vector2.right * edgeRadius;
        Vector2 vectorRightUp = topRight + Vector2.right * edgeRadius;

        GL.Vertex(vectorTopRight);
        GL.Vertex(vectorTopLeft);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.up.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + topLeft);
        }

        GL.Vertex(vectorLeftUp);
        GL.Vertex(vectorLeftDown);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.left.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + bottomLeft);
        }

        GL.Vertex(vectorBottomLeft);
        GL.Vertex(vectorBottomRight);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.down.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + bottomRight);
        }

        GL.Vertex(vectorRightDown);
        GL.Vertex(vectorRightUp);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.right.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + topRight);
        }

        GL.End();
    }

    private static void InternalDrawBoxEdgeRadiusRotated(Vector2 position, Vector2 size, float edgeRadius, float angle, Color? colorSetting = null)
    {
        GL.wireframe = false;
        GL.Begin(GL.LINE_STRIP);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        int num = 8;
        Vector2 halfSize = new Vector2(size.x / 2, size.y / 2);
        Vector2 topRight = position + halfSize.MultiplyEach(1, 1).Rotate(angle);
        Vector2 topLeft = position + halfSize.MultiplyEach(-1, 1).Rotate(angle);
        Vector2 bottomLeft = position + halfSize.MultiplyEach(-1, -1).Rotate(angle);
        Vector2 bottomRight = position + halfSize.MultiplyEach(1, -1).Rotate(angle);

        Vector2 vectorTopRight = topRight + Vector2.up.Rotate(angle) * edgeRadius;
        Vector2 vectorTopLeft = topLeft + Vector2.up.Rotate(angle) * edgeRadius;

        Vector2 vectorLeftUp = topLeft + Vector2.left.Rotate(angle) * edgeRadius;
        Vector2 vectorLeftDown = bottomLeft + Vector2.left.Rotate(angle) * edgeRadius;

        Vector2 vectorBottomLeft = bottomLeft + Vector2.down.Rotate(angle) * edgeRadius;
        Vector2 vectorBottomRight = bottomRight + Vector2.down.Rotate(angle) * edgeRadius;

        Vector2 vectorRightDown = bottomRight + Vector2.right.Rotate(angle) * edgeRadius;
        Vector2 vectorRightUp = topRight + Vector2.right.Rotate(angle) * edgeRadius;

        GL.Vertex(vectorTopRight);
        GL.Vertex(vectorTopLeft);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.up.Rotate(90f * ((float)i / (float)num) + angle) * edgeRadius) + topLeft);
        }

        GL.Vertex(vectorLeftUp);
        GL.Vertex(vectorLeftDown);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.left.Rotate(90f * ((float)i / (float)num) + angle) * edgeRadius) + bottomLeft);
        }

        GL.Vertex(vectorBottomLeft);
        GL.Vertex(vectorBottomRight);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.down.Rotate(90f * ((float)i / (float)num) + angle) * edgeRadius) + bottomRight);
        }

        GL.Vertex(vectorRightDown);
        GL.Vertex(vectorRightUp);
        for (int i = 0; i <= num; i++)
        {
            GL.Vertex((Vector2.right.Rotate(90f * ((float)i / (float)num) + angle) * edgeRadius) + topRight);
        }

        GL.End();
    }

    /// <summary>
    /// Draws an open rectangle at 'position' with 'size' rotated by 'angle'
    /// </summary>
    public static void DrawOpenRect(Vector2 position, Vector2 size, float angle, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawRect(position, size, angle, false, colorSetting));

    /// <summary>
    /// Draws a solid rectangle at 'position' with 'size' rotated by 'angle'
    /// </summary>
    public static void DrawSolidRect(Vector2 position, Vector2 size, float angle, Color? colorSetting = null)
            => drawActions.Add(() => InternalDrawRect(position, size, angle, true, colorSetting));

    private static void InternalDrawRect(Vector2 position, Vector2 size, float angle, bool solid, Color? colorSetting = null)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(size.x / 2, size.y / 2).Rotate(angle) + position);
        points.Add(new Vector2(-size.x / 2, size.y / 2).Rotate(angle) + position);
        points.Add(new Vector2(-size.x / 2, -size.y / 2).Rotate(angle) + position);
        points.Add(new Vector2(size.x / 2, -size.y / 2).Rotate(angle) + position);

        if (!solid)
            InternalDrawPath(points, true, colorSetting);
        else
            InternalDrawFilledPath(points, colorSetting);
    }



    /// <summary>
    /// Draws a line starting at 'from' ending at 'to'
    /// </summary>
    public static void DrawLine(Vector2 from, Vector2 to, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawLine(from, to, colorSetting));

    private static void InternalDrawLine(Vector2 from, Vector2 to, Color? colorSetting = null)
    {
        GL.wireframe = true;
        GL.Begin(GL.LINES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));
        GL.Vertex(from);
        GL.Vertex(to);
        GL.End();
    }

    /// <summary>
    /// Draws a dashed line starting at 'from' ending at 'to'. Length of each dash is 'dashLength' with 'gapLength' space between them
    /// </summary>
    public static void DrawDashedLine(Vector2 from, Vector2 to, float dashLength, float gapLength, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawDashedLine(from, to, dashLength, gapLength, colorSetting));

    private static void InternalDrawDashedLine(Vector2 from, Vector2 to, float dashLength, float gapLength, Color? colorSetting = null)
    {
        GL.wireframe = true;
        GL.Begin(GL.LINES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        float accumulatedDistance = 0;
        float totalDistance = Vector2.Distance(from, to);
        Vector2 point = from;
        Vector2 direction = (to - from).normalized;

        while (accumulatedDistance < totalDistance)
        {
            GL.Vertex(point);
            point += direction * dashLength;
            GL.Vertex(point);
            point += direction * gapLength;

            accumulatedDistance += dashLength + gapLength;
        }

        GL.End();
    }



    /// <summary>
    /// Draws a path connecting the points in 'points'
    /// </summary>
    public static void DrawPath(List<Vector2> points, bool closed = false, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawPath(points, closed, colorSetting));
    private static void InternalDrawPath(List<Vector2> points, bool closed, Color? colorSetting = null)
    {
        if (points == null || points.Count == 0)
            return;

        GL.wireframe = true;
        GL.Begin(GL.LINE_STRIP);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        foreach (Vector2 point in points)
            GL.Vertex(point);

        if (closed)
            GL.Vertex(points[0]);

        GL.End();
    }



    /// <summary>
    /// Draws an open polygon connecting the points in 'vertices'
    /// </summary>
    public static void DrawOpenPolygon(List<Vector2> vertices, Color? colorSetting = null)
    => drawActions.Add(() => InternalDrawPath(vertices, true, colorSetting));

    /// <summary>
    /// Draws a solid polygon connecting the points in 'vertices'
    /// </summary>
    public static void DrawSolidPolygon(List<Vector2> vertices, Color? colorSetting = null)
    => drawActions.Add(() => InternalDrawFilledPath(vertices, colorSetting));

    private static void InternalDrawPolygon(List<Vector2> vertices, bool solid, Color? colorSetting = null)
    {
        if (solid)
            InternalDrawFilledPath(vertices, colorSetting);
        else
            InternalDrawPath(vertices, true, colorSetting);
    }

    private static void InternalDrawFilledPath(List<Vector2> points, Color? colorSetting = null)
    {
        if (points == null || points.Count == 0)
            return;

        GL.wireframe = false;
        GL.Begin(GL.TRIANGLES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        Vector2 center = new Vector2(points.Select(p => p.x).Sum() / points.Count, points.Select(p => p.y).Sum() / points.Count);

        for (int i = 0; i < points.Count; i++)
        {
            GL.Vertex(center);
            GL.Vertex(points[i]);
            GL.Vertex(points[(i + 1) % points.Count]);
        }

        GL.End();
    }



    /// <summary>
    /// Draws a solid polygon connecting the points in 'vertices'. Trianlges colored with 'colors'
    /// </summary>
    public static void DrawMultiColoredPolygon(List<Vector2> points, List<Color> colors)
    => drawActions.Add(() => InternalDrawMultiColoredPolygon(points, colors));
    private static void InternalDrawMultiColoredPolygon(List<Vector2> points, List<Color> colors)
    {
        if (points == null || points.Count == 0)
            return;

        GL.wireframe = false;
        GL.Begin(GL.TRIANGLES);

        bool noColors = colors == null || colors.Count == 0;
        if (noColors)
            GL.Color(color.Value);

        Vector2 center = new Vector2(points.Select(p => p.x).Sum() / points.Count, points.Select(p => p.y).Sum() / points.Count);

        for (int i = 0; i < points.Count; i++)
        {
            if (!noColors)
                GL.Color(colors[i % colors.Count]);

            GL.Vertex(center);
            GL.Vertex(points[i]);
            GL.Vertex(points[(i + 1) % points.Count]);
        }

        GL.End();
    }

    /// <summary>
    /// Draws open triangles using every 3 vertices in 'vertices'
    /// </summary>
    public static void DrawOpenTriangles(Vector2[] vertices, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawTriangle(vertices, false, colorSetting));

    /// <summary>
    /// Draws solid triangles using every 3 vertices in 'vertices'
    /// </summary>
    public static void DrawSolidTriangles(Vector2[] vertices, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawTriangle(vertices, true, colorSetting));
    private static void InternalDrawTriangle(Vector2[] points, bool solid, Color? colorSetting = null)
    {
        GL.wireframe = !solid;
        GL.Begin(GL.TRIANGLES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        foreach (Vector2 point in points)
            GL.Vertex(point);

        GL.End();
    }



    /// <summary>
    /// Draws an open triangle based on a center position, height, and width. Angle rotated the triangle. Skew offsets the point opposite the base edge
    /// </summary>
    public static void DrawOpenTriangle(Vector2 position, Vector2 centerOffset, float height, float width, float skew, float angle, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawTriangleAdv(position, centerOffset, height, width, skew, angle, false, colorSetting));

    /// <summary>
    /// Draws a solid triangle based on a center position, height, and width. Angle rotated the triangle. Skew offsets the point opposite the base edge
    /// </summary>
    public static void DrawSolidTriangle(Vector2 position, Vector2 centerOffset, float height, float width, float skew, float angle, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawTriangleAdv(position, centerOffset, height, width, skew, angle, true, colorSetting));

    private static void InternalDrawTriangleAdv(Vector2 position, Vector2 centerOffset, float height, float width, float skew, float angle, bool solid, Color? colorSetting = null)
    {
        GL.wireframe = !solid;
        GL.Begin(GL.TRIANGLES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        float adjustedSkew = MathEX.Remap(-1, 1, 0, 1, skew);
        Vector2 adjustedOffset = centerOffset * height / 2;

        List<Vector2> points = new(); // with center at 0,0
        points.Add(new Vector2(-width / 2, -height / 2));
        points.Add(new Vector2(width / 2, -height / 2));
        points.Add(new Vector2(Mathf.LerpUnclamped(-width / 2, width / 2, adjustedSkew), height / 2));

        foreach (Vector2 point in points)
            GL.Vertex(point.Rotate(angle) + position + adjustedOffset.Rotate(angle));

        GL.End();
    }



    /// <summary>
    /// Draws an open circle at 'position' with 'radius' by drawing a polygon with 'numEdges' (automatically calculated if 0)
    /// </summary>
    public static void DrawOpenCircle(Vector2 position, float radius, int numEdges = 0, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawEdgeCircle(position, radius, 360, 0, numEdges, false, ArcCloseType.None, colorSetting));

    /// <summary>
    /// Draws an open arc with 'angle' at 'position' with 'radius' by drawing a polygon with 'numEdges' (automatically calculated if 0)
    /// </summary>
    public static void DrawOpenArc(Vector2 position, float radius, float arcAngle, float offsetAngle, ArcCloseType arcCloseType = ArcCloseType.None, int numEdges = 0, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawEdgeCircle(position, radius, arcAngle, offsetAngle, numEdges, false, arcCloseType, colorSetting));

    /// <summary>
    /// Draws an open circle at 'position' with 'radius' by drawing a polygon with 'numEdges', drawing only half the edges
    /// </summary>
    public static void DrawDashedCircle(Vector2 position, float radius, int numEdges = 0, Color? colorSetting = null)
    => drawActions.Add(() => InternalDrawEdgeCircle(position, radius, 360, 0, numEdges, true, ArcCloseType.None, colorSetting));

    private static void InternalDrawCircle(Vector2 position, float radius, float arcAngle, float offsetAngle, int numEdges, bool solid, ArcCloseType arcCloseType = ArcCloseType.None, Color? colorSetting = null)
    {
        if (solid)
            InternalDrawFilledCircle(position, radius, arcAngle, offsetAngle, numEdges, ArcCloseType.None, colorSetting);
        else
            InternalDrawEdgeCircle(position, radius, arcAngle, offsetAngle, numEdges, false, ArcCloseType.None, colorSetting);
    }

    private static void InternalDrawEdgeCircle(Vector2 position, float radius, float arcAngle, float offsetAngle, int numEdges, bool dashed, ArcCloseType arcCloseType = ArcCloseType.None, Color? colorSetting = null)
    {
        GL.wireframe = true;
        GL.Begin(!dashed ? GL.LINE_STRIP : GL.LINES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        if (numEdges <= 0)
            numEdges = (int)(12 * Mathf.Sqrt(radius * 2) / (360f / arcAngle));

        for (int i = 0; i <= numEdges; i++)
            GL.Vertex(position + Vector2.right.Rotate(arcAngle * ((float)i / (float)numEdges) + offsetAngle) * radius);

        switch (arcCloseType)
        {
            case ArcCloseType.Flat:
                GL.Vertex(position + Vector2.right.Rotate(offsetAngle) * radius);
                break;
            case ArcCloseType.Center:
                GL.Vertex(position);
                GL.Vertex(position + Vector2.right.Rotate(offsetAngle) * radius);
                break;
            case ArcCloseType.Edge:
                GL.Vertex(EdgeMinMaxPoint(position, position + Vector2.right.Rotate(offsetAngle) * radius, position + Vector2.right.Rotate(arcAngle + offsetAngle) * radius, arcAngle));
                GL.Vertex(position + Vector2.right.Rotate(offsetAngle) * radius);
                break;

        }

        GL.End();
    }

    /// <summary>
    /// Approximates a solid circle at 'position' with 'radius' by drawing a polygon with 'numEdges' (automatically calculated if 0)
    /// </summary>
    public static void DrawSolidCircle(Vector2 position, float radius, int numEdges = 0, Color? colorSetting = null)
    => drawActions.Add(() => InternalDrawFilledCircle(position, radius, 360, 0, numEdges, ArcCloseType.None, colorSetting));

    /// <summary>
    /// Draws a solid arc with 'angle' at 'position' with 'radius' by drawing a polygon with 'numEdges' (automatically calculated if 0)
    /// </summary>
    public static void DrawSolidArc(Vector2 position, float radius, float arcAngle, float offsetAngle, ArcCloseType arcCloseType = ArcCloseType.Center, int numEdges = 0, Color? colorSetting = null)
    => drawActions.Add(() => InternalDrawFilledCircle(position, radius, arcAngle, offsetAngle, numEdges, arcCloseType, colorSetting));

    private static void InternalDrawFilledCircle(Vector2 position, float radius, float arcAngle, float offsetAngle, int numEdges, ArcCloseType arcCloseType = ArcCloseType.Center, Color? colorSetting = null)
    {
        GL.wireframe = false;
        GL.Begin(GL.TRIANGLES);
        GL.Color((Color)(colorSetting == null ? color : colorSetting));

        if (numEdges <= 0)
            numEdges = (int)(20 * Mathf.Sqrt(radius * 2) / (360f / arcAngle));

        Vector2 drawStartPosition = position;
        switch (arcCloseType)
        {
            case ArcCloseType.Flat:
                drawStartPosition = Vector2.Lerp(position + Vector2.right.Rotate(offsetAngle) * radius, position + Vector2.right.Rotate(arcAngle + offsetAngle) * radius, .5f);
                break;
            case ArcCloseType.Edge:
                drawStartPosition = EdgeMinMaxPoint(position, position + Vector2.right.Rotate(offsetAngle) * radius, position + Vector2.right.Rotate(arcAngle + offsetAngle) * radius, arcAngle);
                break;
        }

        for (int i = 0; i < numEdges; i++)
        {
            GL.Vertex(drawStartPosition);
            GL.Vertex(position + Vector2.right.Rotate(arcAngle * ((float)i / (float)numEdges) + offsetAngle) * radius);
            GL.Vertex(position + Vector2.right.Rotate(arcAngle * ((float)(i + 1) / (float)numEdges) + offsetAngle) * radius);
        }

        GL.End();
    }

    private static Vector2 EdgeMinMaxPoint(Vector2 center, Vector2 point1, Vector2 point2, float angle)
    {
        Vector2 minXminY = new Vector2(Mathf.Min(point1.x, point2.x), Mathf.Min(point1.y, point2.y));
        Vector2 minXmaxY = new Vector2(Mathf.Min(point1.x, point2.x), Mathf.Max(point1.y, point2.y));
        Vector2 maxXminY = new Vector2(Mathf.Max(point1.x, point2.x), Mathf.Min(point1.y, point2.y));
        Vector2 maxXmaxY = new Vector2(Mathf.Max(point1.x, point2.x), Mathf.Max(point1.y, point2.y));
        Vector2[] vectors = new Vector2[4] { minXminY, minXmaxY, maxXminY, maxXmaxY };

        float distanceMinMin = Vector2.Distance(minXminY, center);
        float distanceMinMax = Vector2.Distance(minXmaxY, center);
        float distanceMaxMin = Vector2.Distance(maxXminY, center);
        float distanceMaxMax = Vector2.Distance(maxXmaxY, center);
        float[] distances = new float[4] { distanceMinMin, distanceMinMax, distanceMaxMin, distanceMaxMax };

        float minDistance = Mathf.Min(Mathf.Min(Mathf.Min(distanceMinMin, distanceMinMax), distanceMaxMin), distanceMaxMax);
        float maxDistance = Mathf.Max(Mathf.Max(Mathf.Max(distanceMinMin, distanceMinMax), distanceMaxMin), distanceMaxMax);
        int index = Array.IndexOf(distances, (Mathf.Abs(angle) % 360) <= 180 ? minDistance : maxDistance);

        return vectors[index];
    }

    private static Vector2 GetPointOnUnitCircle(float radius, float angle) => Vector2.right.Rotate(angle) * radius;




    /// <summary>
    /// Draws an open capsule at 'position' based on box size and capsule direction
    /// </summary>
    public static void DrawOpenCapsule(Vector2 position, Vector2 size, CapsuleDirection2D direction, float angle = 0, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawCapsule(position, size, direction, angle, false, colorSetting));

    /// <summary>
    /// Draws an open capsule starting at 'from' ending at 'to' with 'radius'
    /// </summary>
    public static void DrawOpenCapsule(Vector2 from, Vector2 to, float radius, Color? colorSetting = null)
    {
        Vector2 center = Vector2.Lerp(from, to, .5f);
        drawActions.Add(() => InternalDrawCapsule(center, new Vector2(radius * 2, Vector2.Distance(from, to) + radius * 2), CapsuleDirection2D.Vertical, Vector2.SignedAngle(Vector2.up, from - center), false, colorSetting));
    }

    /// <summary>
    /// Draws a solid capsule at 'position' based on box size and capsule direction
    /// </summary>
    public static void DrawSolidCapsule(Vector2 position, Vector2 size, CapsuleDirection2D direction, float angle = 0, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawCapsule(position, size, direction, angle, true, colorSetting));

    /// <summary>
    /// Draws a solid capsule starting at 'from' ending at 'to' with 'radius'
    /// </summary>
    public static void DrawSolidCapsule(Vector2 from, Vector2 to, float radius, Color? colorSetting = null)
    {
        Vector2 center = Vector2.Lerp(from, to, .5f);
        drawActions.Add(() => InternalDrawCapsule(center, new Vector2(radius * 2, Vector2.Distance(from, to) + radius * 2), CapsuleDirection2D.Vertical, Vector2.SignedAngle(Vector2.up, from - center), true, colorSetting));
    }

    private static void InternalDrawCapsule(Vector2 position, Vector2 size, CapsuleDirection2D direction, float angle, bool solid, Color? colorSetting = null)
    {
        float radius = direction == CapsuleDirection2D.Vertical ? size.x / 2 : size.y / 2;
        float difference = direction == CapsuleDirection2D.Vertical ?
            (size.y > size.x ? (size.y - size.x) / 2 : 0) :
            (size.x > size.y ? (size.x - size.y) / 2 : 0);

        float offsetAngle = (direction == CapsuleDirection2D.Vertical ? 0 : 90) + angle;
        Vector2 curveOffsetDirection = (direction == CapsuleDirection2D.Vertical ? Vector2.up : Vector2.left).Rotate(angle);

        if (!solid)
        {
            InternalDrawEdgeCircle(position + (curveOffsetDirection * difference), radius, 180, offsetAngle, 0, false, ArcCloseType.None, colorSetting);
            InternalDrawEdgeCircle(position + (-curveOffsetDirection * difference), radius, 180, 180 + offsetAngle, 0, false, ArcCloseType.None, colorSetting);

            Vector2 orientationSize = (direction == CapsuleDirection2D.Vertical ? Vector2.up : Vector2.left).Rotate(angle);
            InternalDrawLine(position + (orientationSize * difference) + GetPointOnUnitCircle(radius, 180 + offsetAngle),
                             position + (-orientationSize * difference) + GetPointOnUnitCircle(radius, 180 + offsetAngle));
            InternalDrawLine(position + (orientationSize * difference) + GetPointOnUnitCircle(radius, offsetAngle),
                             position + (-orientationSize * difference) + GetPointOnUnitCircle(radius, offsetAngle));
        }
        else
        {
            InternalDrawFilledCircle(position + (curveOffsetDirection * difference), radius, 180, offsetAngle, 0, ArcCloseType.Center, colorSetting);
            InternalDrawFilledCircle(position + (-curveOffsetDirection * difference), radius, 180, 180 + offsetAngle, 0, ArcCloseType.Center, colorSetting);

            Vector2 orientationSize = direction == CapsuleDirection2D.Vertical ? Vector2.up : Vector2.right;
            InternalDrawRect(position, (size - (radius * 2 * orientationSize)).ZeroNegatives(), angle, solid);
        }
    }

    public static void InternalDrawCapsule(Vector2 from, Vector2 to, float radius, Color? colorSetting = null)
    {
        Vector2 center = Vector2.Lerp(from, to, .5f);
        InternalDrawCapsule(center, new Vector2(radius * 2, Vector2.Distance(from, to) + radius * 2), CapsuleDirection2D.Vertical, Vector2.SignedAngle(Vector2.up, from - center), true, colorSetting);
    }

    /// <summary>
    /// Draws a solid path with 'thickness' connecting the points in 'points'
    /// </summary>
    public static void DrawCapsulePath(List<Vector2> points, float thickness, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawCapsulePath(points, thickness, colorSetting));
    private static void InternalDrawCapsulePath(List<Vector2> points, float thickness, Color? colorSetting = null)
    {
        if (points == null || points.Count == 0 || thickness == 0)
            return;

        for (int i = 1; i < points.Count; i++)
            InternalDrawCapsule(points[i - 1], points[i], thickness / 2, colorSetting);
    }




    /// <summary>
    /// Draws a bezier curve starting at 'from' ending at 'to'. Curve [typically between -1 and 1]
    /// </summary>
    public static void DrawBezier(Vector2 from, Vector2 to, float curve = .75f, int numEdges = 0, Color? colorSetting = null)
        => drawActions.Add(() => InternalDrawBezier(from, to, curve, numEdges, colorSetting));

    private static void InternalDrawBezier(Vector2 from, Vector2 to, float curve, int numEdges, Color? colorSetting = null)
    {
        List<Vector2> joints = new List<Vector2>();

        float lerpCenter = MathEX.Remap(-1, 1, 0, 1, curve);

        Vector2 p1c = new Vector2(from.x, to.y);
        Vector2 p4c = new Vector2(to.x, from.y);

        Vector2 p2 = Vector2.LerpUnclamped(p1c, p4c, lerpCenter);
        Vector2 p3 = Vector2.LerpUnclamped(p1c, p4c, 1 - lerpCenter);

        if (numEdges <= 0)
            numEdges = (int)Mathf.Clamp(Mathf.Pow(25, Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(curve))))), 1, 75);

        float t = 0;
        while (t < 1)
        {
            Vector2 point = Mathf.Pow(1 - t, 3) * from +
                            3 * Mathf.Pow(1 - t, 2) * t * p2 +
                            3 * (1 - t) * Mathf.Pow(t, 2) * p3 +
                            Mathf.Pow(t, 3) * to;
            joints.Add(point);
            t += (1f / (float)numEdges);
        }

        t = 1;
        Vector2 finalPoint = Mathf.Pow(1 - t, 3) * from +
                            3 * Mathf.Pow(1 - t, 2) * t * p2 +
                            3 * (1 - t) * Mathf.Pow(t, 2) * p3 +
                            Mathf.Pow(t, 3) * to;
        joints.Add(finalPoint);

        InternalDrawPath(joints, false, colorSetting);
    }





    /// <summary>
    /// Draws any 2D collider shape
    /// </summary>
    public static void DrawCollider2D(Collider2D collider, bool solid = false) => drawActions.Add(() => InternalDrawCollider2D(collider, solid));
    private static void InternalDrawCollider2D(Collider2D collider, bool solid)
    {
        if (collider is BoxCollider2D)
        {
            BoxCollider2D boxCollider = (BoxCollider2D)collider;

            if (boxCollider.transform.rotation.eulerAngles.z == 0)
            {
                InternalDrawBox((Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size, solid);
                if (boxCollider.edgeRadius > 0)
                    InternalDrawBoxEdgeRadius((Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size, boxCollider.edgeRadius);
            }
            else
            {
                InternalDrawRect((Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size, boxCollider.transform.rotation.eulerAngles.z, solid);
                if (boxCollider.edgeRadius > 0)
                    InternalDrawBoxEdgeRadiusRotated((Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size, boxCollider.edgeRadius, boxCollider.transform.rotation.eulerAngles.z);
            }
        }
        else if (collider is CompositeCollider2D)
        {
            CompositeCollider2D compositeCollider = (CompositeCollider2D)collider;

            for (int i = 0; i < compositeCollider.pathCount; i++)
            {
                Vector2[] array = new Vector2[compositeCollider.GetPathPointCount(i)];
                compositeCollider.GetPath(i, array);

                InternalDrawPolygon(array.Select(x => x.Rotate(compositeCollider.transform.rotation.eulerAngles.z) + (Vector2)compositeCollider.transform.position).ToList(), solid);
            }
        }
        else if (collider is CircleCollider2D)
        {
            CircleCollider2D circleCollider = (CircleCollider2D)collider;
            InternalDrawCircle((Vector2)circleCollider.transform.position + circleCollider.offset, circleCollider.radius, 360f, 0f, 0, solid, ArcCloseType.None);
        }
        else if (collider is CapsuleCollider2D)
        {
            CapsuleCollider2D capsuleCollider2D = (CapsuleCollider2D)collider;
            InternalDrawCapsule((Vector2)capsuleCollider2D.transform.position + capsuleCollider2D.offset, capsuleCollider2D.size, capsuleCollider2D.direction, capsuleCollider2D.transform.rotation.eulerAngles.z, solid);
        }
        else if (collider is PolygonCollider2D)
        {
            PolygonCollider2D polygonCollider2D = (PolygonCollider2D)collider;
            InternalDrawPolygon(polygonCollider2D.points.Select(x => x.Rotate(polygonCollider2D.transform.rotation.eulerAngles.z) + (Vector2)polygonCollider2D.transform.position).ToList().ToList(), solid);
        }
        else if (collider is EdgeCollider2D)
        {
            EdgeCollider2D edgeCollider2D = (EdgeCollider2D)collider;
            InternalDrawPath(edgeCollider2D.points.Select(x => x.Rotate(edgeCollider2D.transform.rotation.eulerAngles.z) + (Vector2)edgeCollider2D.transform.position).ToList(), false);
        }
        else
        {
            Debug.LogError($"GLGizmos cannot draw type of {collider.GetType()}");
        }
    }


    void CreateGLMaterial()
    {
        if (GLmat == null)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            GLmat = new Material(shader);
            GLmat.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            GLmat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            GLmat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            GLmat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            GLmat.SetInt("_ZWrite", 0);
        }
    }

    void DestroyGLMaterial()
    {
        if (GLmat != null)
        {
            if (Application.isPlaying)
            {
                Destroy(GLmat);
            }
            else
            {
                DestroyImmediate(GLmat);
            }
        }
    }
}

//public static class Extensions
//{
//    // MathEX
//    public enum AngleUnits { Degrees, Radians };

//    static float AngleUnitConversion(float value, AngleUnits unitsFrom, AngleUnits unitsTo)
//    {
//        string unitString = "" + (int)unitsFrom + "" + (int)unitsTo;
//        switch (unitString)
//        {
//            case "01": /*Degrees -> Radians*/ return Mathf.Deg2Rad * value;
//            case "10": /*Radians -> Degrees*/ return Mathf.Rad2Deg * value;
//            default: return value;
//        }
//    }

//    public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
//    {
//        float t = InverseLerp(iMin, iMax, value);
//        return Lerp(oMin, oMax, t);
//    }
//    static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
//    static float Lerp(float a, float b, float t) => (1f - t) * a + t * b;

//    // Vector Extensions
//    public static Vector2 Rotate90CW(this Vector2 v) => new Vector2(v.y, -v.x);
//    public static Vector2 Rotate90CCW(this Vector2 v) => new Vector2(-v.y, v.x);

//    public static Vector2 Rotate(this Vector2 v, float angle, AngleUnits units = AngleUnits.Degrees)
//    {
//        angle = AngleUnitConversion(angle, units, AngleUnits.Radians);

//        float ca = Mathf.Cos(angle);
//        float sa = Mathf.Sin(angle);
//        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
//    }

//    public static Vector2 MultiplyEach(this Vector2 v, float scaleX, float scaleY) => new Vector2(v.x * scaleX, v.y * scaleY);
//    public static Vector2 ZeroNegatives(this Vector2 v) => new Vector2(v.x > 0 ? v.x : 0, v.y > 0 ? v.y : 0);
//}
