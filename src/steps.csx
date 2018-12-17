
using System.Reflection;

public delegate void Step();

public delegate Task AsyncStep();

public delegate void SummaryStep(IEnumerable<StepResult> results);


Action stepsDummyaction = () => StepsDummy();

WriteLine(stepsDummyaction.Target.GetType());

public void StepsDummy(){}


StepRunner.Initialize(stepsDummyaction.Target);


public static void ShowHelp(this StepInfo[] steps)
{
    WriteLine("---------------------------------------------------------------------");
    WriteLine("Available Steps");
    WriteLine("---------------------------------------------------------------------");
    var stepMaxWidth = steps.Select(s => $"{s.Name}".Length).OrderBy(l => l).Last() + 15;
    var descriptionMaxWidth = steps.Select(s => $"{s.Description}".Length).OrderBy(l => l).Last();
    Write("Step".PadRight(stepMaxWidth, ' '));
    WriteLine("Description");

    Write("".PadRight(stepMaxWidth,'-'));
    WriteLine("".PadRight(descriptionMaxWidth,'-'));
    foreach(var step in steps)
    {
        var name = step.Name + (step.IsDefault ? " (default)" : string.Empty);
        Write(name.PadRight(stepMaxWidth, ' '));
        WriteLine(step.Description);
    }
}

public static void ShowSummary(this StepResult[] results)
{

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
        var stepDelegates2 = GetStepDelegates();
        var results = new List<StepResult>();

        SummaryStep summaryStep = GetSummaryStepDelegate();

        if (stepDelegates2.Keys.Intersect(stepNames).Count() == 0)
        {
            var defaultDelegate = GetDefaultDelegate(stepDelegates2);
            if (defaultDelegate != null)
            {

            }
            stepDelegates2.Values.ToArray().ShowHelp();
        }

        foreach(var stepName in stepNames)
        {
            if (stepName.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                stepDelegates2.Values.ToArray().ShowHelp();
                continue;
            }

            if (stepDelegates2.TryGetValue(stepName, out var stepDelegate))
            {
                results.Add(await stepDelegate.Invoke());
                continue;
            }
        }

        summaryStep(results);
    }


    private static SummaryStep GetSummaryStepDelegate()
    {
        var summarySteps = GetStepDelegates<SummaryStep>();
        if (summarySteps.Length  > 1)
        {
            throw new InvalidOperationException("Found multiple summary steps");
        }
        if (summarySteps.Length == 1)
        {
            return summarySteps[0];
        }

        return results => results.ToArray().ShowSummary();
    }

    private static Func<Task> GetDefaultDelegate(Dictionary<string, StepInfo> stepDelegates)
    {
        var defaultStepDelegate = stepDelegates.Values.Where(si => si.IsDefault).SingleOrDefault();
        if (defaultStepDelegate != null)
        {
            return () => defaultStepDelegate.Invoke();
        }

        return null;

    }

    private static Dictionary<string, StepInfo> GetStepDelegates()
    {
        var stepFields = GetStepFields<Step>();
        List<StepInfo> results = new List<StepInfo>();
        foreach(var stepField in stepFields)
        {
            StepInfo stepInfo = new StepInfo(stepField.Name, GetStepDescription(stepField), RepresentsDefaultStep(stepField), () => {GetStepDelegate<Step>(stepField)(); return Task.CompletedTask;});
            results.Add(stepInfo);
        }

        var asyncStepFields = GetStepFields<AsyncStep>();
        foreach (var asyncStepField in asyncStepFields)
        {
            StepInfo stepInfo = new StepInfo(asyncStepField.Name, GetStepDescription(asyncStepField), RepresentsDefaultStep(asyncStepField), () => GetStepDelegate<AsyncStep>(asyncStepField)());
            results.Add(stepInfo);
        }

        return results.ToDictionary(si => si.Name, si => si, StringComparer.OrdinalIgnoreCase);
    }

    private static TStep GetStepDelegate<TStep>(FieldInfo stepField)
    {
        return (TStep)(stepField.IsStatic ? stepField.GetValue(null) : stepField.GetValue(_submission));
    }

    private static TStep GetStepDelegate<TStep>(PropertyInfo property)
    {
        return (TStep)(property.GetMethod.IsStatic ? property.GetValue(null) : property.GetValue(_submission));
    }

    private static TStep[] GetStepDelegates<TStep>()
    {
        var fieldSteps =  GetStepFields<TStep>().Select(f => GetStepDelegate<TStep>(f));
        var propertySteps = GetStepProperties<TStep>().Select(p => GetStepDelegate<TStep>(p));
        return fieldSteps.Concat(propertySteps).ToArray();
    }

    private static string GetStepDescription(MemberInfo stepFieldInfo)
    {
        return stepFieldInfo.GetCustomAttribute<StepDescriptionAttribute>()?.Description ?? string.Empty;
    }

    private static bool RepresentsDefaultStep(MemberInfo memberInfo)
    {
        return memberInfo.IsDefined(typeof(DefaultStepAttribute)) || memberInfo.Name.Equals("defaultstep", StringComparison.OrdinalIgnoreCase);
    }

    private static FieldInfo[] GetStepFields<TStep>()
    {
        return _submissionType.GetFields().Where(f => f.FieldType == typeof(TStep)).ToArray();
    }

    private static PropertyInfo[] GetStepProperties<TStep>()
    {
        return _submissionType.GetProperties().Where(f => f.PropertyType == typeof(TStep)).ToArray();
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


public class StepInfo
{
    private readonly Func<Task> _step;

    public StepInfo(string name, string description, bool isDefault, Func<Task> step)
    {
        Name = name;
        Description = description;
        IsDefault = isDefault;
        _step = step;
    }

    public string Name { get; }
    public string Description { get; }
    public bool IsDefault { get; }

    public async Task<StepResult> Invoke()
    {
        var stopWatch = Stopwatch.StartNew();
        await _step();
        return new StepResult(Name, stopWatch.Elapsed);
    }
}





