
using System.Reflection;

public delegate void Step();

public delegate Task AsyncStep();

Action stepsDummyaction = () => StepsDummy();

WriteLine(stepsDummyaction.Target.GetType());

public void StepsDummy(){}


StepRunner.Initialize(stepsDummyaction.Target);


public static void ShowHelp(this StepInfo[] steps)
{
    WriteLine("---------------------------------------------------------------------");
    WriteLine("Available Steps");
    WriteLine("---------------------------------------------------------------------");
    var stepMaxWidth = steps.Select(s => $"{s.Name}".Length).OrderBy(l => l).Last() + 10;
    var descriptionMaxWidth = steps.Select(s => $"{s.Description}".Length).OrderBy(l => l).Last();
    Write("Step".PadRight(stepMaxWidth, ' '));
    WriteLine("Description");

    Write("".PadRight(stepMaxWidth,'-'));
    WriteLine("".PadRight(descriptionMaxWidth,'-'));
    foreach(var step in steps)
    {
        Write(step.Name.PadRight(stepMaxWidth, ' '));
        WriteLine(step.Description);
    }
}




public static class StepRunner
{
    private static object _submission;
    private static Type _submissionType;

    internal static void Initialize(object submission)
    {
        _submission = submission;
        _submissionType = submission.GetType();
    }

    //Drop the default stuff. Search for a "default" delegate

    //Also create a summary delegate and a usage delegate that users can use to format summary and display help.

    //public async static Task Execute(params string[] steps, AsyncStep defaultStep = null)


    public async static Task Execute(IList<string> stepNames)
    {
        await ExecuteSteps(stepNames.ToArray());
    }

    private async static Task ExecuteSteps(string[] stepNames)
    {
        var stepDelegates = GetStepDelegates<Step>().ToDictionary(si => si.Name, si => si, StringComparer.OrdinalIgnoreCase);
        var asyncStepDelegates = GetStepDelegates<AsyncStep>().ToDictionary(si => si.Name, si => si, StringComparer.OrdinalIgnoreCase);
        var results = new List<StepResult>();

        if (stepDelegates.Keys.Intersect(stepNames).Count() == 0 && asyncStepDelegates.Keys.Intersect(stepNames).Count() == 0)
        {

        }

        foreach(var stepName in stepNames)
        {
            if (stepName.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                stepDelegates.Values.Cast<StepInfo>().Concat(asyncStepDelegates.Values.Cast<StepInfo>()).ToArray().ShowHelp();
                continue;
            }

            if (stepDelegates.TryGetValue(stepName, out var stepDelegate))
            {
                stepDelegate.Step();
                continue;
            }
            if (asyncStepDelegates.TryGetValue(stepName, out var asyncStepDelegate))
            {
                await asyncStepDelegate.Step();
            }
        }
    }


    private static StepInfo<TStep>[] GetStepDelegates<TStep>()
    {
        var fieldSteps =  GetStepFields<TStep>().Select(f => GetStepInfo<TStep>(f));
        var propertySteps = GetStepProperties<TStep>().Select(p => GetStepInfo<TStep>(p));
        return fieldSteps.Concat(propertySteps).ToArray();
    }

    private static StepInfo<TStep> GetStepInfo<TStep>(PropertyInfo property)
    {
        return new StepInfo<TStep>(property.Name, (TStep)(property.GetMethod.IsStatic ? property.GetValue(null) : property.GetValue(_submission)), GetStepDescription(property), RepresentsDefaultStep(property));
    }

    private static StepInfo<TStep> GetStepInfo<TStep>(FieldInfo field)
    {
        return new StepInfo<TStep>(field.Name,(TStep)(field.IsStatic ? field.GetValue(null) : field.GetValue(_submission)), GetStepDescription(field), RepresentsDefaultStep(field));
    }

    private static string GetStepDescription(MemberInfo stepFieldInfo)
    {
        return stepFieldInfo.GetCustomAttribute<StepDescriptionAttribute>()?.Description ?? string.Empty;
    }

    private static bool RepresentsDefaultStep(MemberInfo memberInfo)
    {
        return memberInfo.IsDefined(typeof(DefaultStepAttribute)) && memberInfo.Name.Equals("defaultstep", StringComparison.OrdinalIgnoreCase);
    }

    private static FieldInfo[] GetStepFields<TStep>()
    {
        return _submissionType.GetFields().Where(f => f.FieldType == typeof(TStep)).ToArray();
    }

    private static PropertyInfo[] GetStepProperties<TStep>()
    {
        return _submissionType.GetProperties().Where(f => f.PropertyType == typeof(TStep)).ToArray();
    }





    public class StepResult
    {
        public StepResult(string name, TimeSpan duration)
        {
            Name = name;
            Duration = duration;
        }

        public string Name { get; }
        public TimeSpan Duration { get; }
    }



}

[AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class StepDescriptionAttribute : Attribute
{
    public StepDescriptionAttribute(string description)
    {
        Description = description;
    }

    public string Description { get; }
}

public sealed class DefaultStepAttribute : Attribute
{
    public DefaultStepAttribute()
    {
    }
}

public class StepInfo
{
    public StepInfo(string name, string description, bool isDefault)
    {
        Name = name;
        Description = description;
        IsDefault = isDefault;
    }

    public string Name { get; }
    public string Description { get; }
    public bool IsDefault { get; }
}

public class StepInfo<TStep> : StepInfo
    {
        public StepInfo(string name, TStep step, string description, bool isDefault) : base(name, description, isDefault)
        {
            Step = step;
        }

        public TStep Step { get; }

    }