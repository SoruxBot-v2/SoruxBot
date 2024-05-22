namespace SoruxBot.Kernel.Services.PluginService.Model;

public class PluginActionParameter(bool isOptional, Type parameterType, string name)
{
    /// <summary>
    /// 是否是可选的参数
    /// </summary>
    public bool IsOptional { get; set; } = isOptional;

    /// <summary>
    /// 参数类型
    /// </summary>
    public Type ParameterType { get; set; } = parameterType;

    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; set; } = name;
}