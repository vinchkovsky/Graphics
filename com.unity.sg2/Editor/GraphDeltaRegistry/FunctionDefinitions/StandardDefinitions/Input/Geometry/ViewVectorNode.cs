using Usage = UnityEditor.ShaderGraph.GraphDelta.GraphType.Usage;

namespace UnityEditor.ShaderGraph.Defs
{
    internal class ViewVectorNode : IStandardNode
    {
        public static string Name => "ViewVector";
        public static int Version => 1;
        public static NodeDescriptor NodeDescriptor => new(
            Version,
            Name,
            functions: new FunctionDescriptor[] {
                new(
                    "World",
@"  Out = CameraPosWS - GetAbsolutePositionWS(PositionWS);
    if(!IsPerspectiveProjection())
    {
        Out = GetViewForwardDir() * dot(Out, GetViewForwardDir());
    }",
                    new ParameterDescriptor[]
                    {
                        new ParameterDescriptor("Out", TYPE.Vec3, Usage.Out),
                        new ParameterDescriptor("PositionWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Position),
                        new ParameterDescriptor("CameraPosWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_CameraPosition)
                    }
                ),
                new(
                    "Object",
@"  Out = CameraPosWS - GetAbsolutePositionWS(PositionWS);
    if(!IsPerspectiveProjection())
    {
        Out = GetViewForwardDir() * dot(Out, GetViewForwardDir());
    }
    Out = TransformWorldToObjectDir(Out, false);",
                    new ParameterDescriptor[]
                    {
                        new ParameterDescriptor("Out", TYPE.Vec3, Usage.Out),
                        new ParameterDescriptor("PositionWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Position),
                        new ParameterDescriptor("CameraPosWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_CameraPosition)
                    }
                ),
                new(
                    "View",
@"  if(IsPerspectiveProjection())
    {
        Out = -PositionVS;
    }
    else
    {
        Out.x = 0.0f;
        Out.y = 0.0f;
        Out.z = PositionVS.z;
    }",
                    new ParameterDescriptor[]
                    {
                        new ParameterDescriptor("Out", TYPE.Vec3, Usage.Out),
                        new ParameterDescriptor("PositionVS", TYPE.Vec3, Usage.Local, REF.ViewSpace_Position)
                    }
                ),
                new(
                    "Tangent",
@"  basisTransform[0] = TangentWS;
    basisTransform[1] = BitangentWS;
    basisTransform[2] = NormalWS;
    Out = CameraPosWS - GetAbsolutePositionWS(PositionWS);
    if(!IsPerspectiveProjection())
    {
        Out = GetViewForwardDir() * dot(Out, GetViewForwardDir());
    }
    Out = length(Out) * TransformWorldToTangent(Out, basisTransform);",
                    new ParameterDescriptor[]
                    {
                        new ParameterDescriptor("Out", TYPE.Vec3, Usage.Out),
                        new ParameterDescriptor("basisTransform", TYPE.Mat3, Usage.Local),
                        new ParameterDescriptor("NormalWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Normal),
                        new ParameterDescriptor("TangentWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Tangent),
                        new ParameterDescriptor("BitangentWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Bitangent),
                        new ParameterDescriptor("PositionWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_Position),
                        new ParameterDescriptor("CameraPosWS", TYPE.Vec3, Usage.Local, REF.WorldSpace_CameraPosition)
                    }
                )
            }
        );

        public static NodeUIDescriptor NodeUIDescriptor => new(
            Version,
            Name,
            tooltip: "Creates a vector from the current point to the camera position (not normalized)",
            category: "Input/Geometry",
            synonyms: new string[2] { "eye vector", "camera vector" },
            displayName: "View Vector",
            selectableFunctions: new()
            {
                { "World", "World" },
                { "Object", "Object" },
                { "View", "View" },
                { "Tangent", "Tangent" }
            },
            parameters: new ParameterUIDescriptor[2] {
                new ParameterUIDescriptor(
                    name: "ViewDir"
                ),
                new ParameterUIDescriptor(
                    name: "Out",
                    tooltip: "a vector from the current point to the camera position (not normalized)"
                )
            }
        );
    }
}
