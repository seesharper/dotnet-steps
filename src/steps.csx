
//NOTE : Add handling of no default delegate
// ALSO execute if only one delegate.


using System.ComponentModel;
using System.Reflection;

/// <summary>
/// Represents a synchronous step.
/// </summary>
public delegate void Step();

/// <summary>
/// Represents an asynchronous step.
/// </summary>
/// <returns></returns>
public delegate Task AsyncStep();

/// <summary>
/// Represents a function that displays a summary report.
/// </summary>
/// <param name="results"></param>
public delegate void SummaryStep(IEnumerable<StepResult> results);

// Dummy lambda to obtain the submission instance.
Action stepsDummyaction = () => StepsDummy();

void StepsDummy(){}


StepRunner.Initialize(stepsDummyaction.Target);

public static async Task ExecuteSteps(IList<string> args)
{
    await StepRunner.Execute(args);
}


public static void ShowHelp(this StepInfo[] steps)
{
    WriteLine("---------------------------------------------------------------------");
    WriteLine("Available Steps");
    WriteLine("---------------------------------------------------------------------");
    var stepMaxWidth = steps.Select(s => $"{s.Name}".Length).OrderBy(l => l).Last() + 15;
    WriteLine($"{"Step".PadRight(stepMaxWidth)}Description");
    WriteLine($"{"".PadRight(stepMaxWidth - 15,'-')}{"".PadLeft(15)}{"".PadRight(18, '-')}");
    foreach(var step in steps)
    {
        var name = step.Name + (step.IsDefault ? " (default)" : string.Empty);
        Write(name.PadRight(stepMaxWidth, ' '));
        WriteLine(step.Description);
    }
}

public static void ShowSummary(this StepResult[] results)
{
    if (results.Length == 0)
    {
        return;
    }

    WriteLine("---------------------------------------------------------------------");
    WriteLine("Steps Summary");
    WriteLine("---------------------------------------------------------------------");
    var stepMaxWidth = results.Select(s => $"{s.Name}".Length).OrderBy(l => l).Last() + 15;
    WriteLine($"{"Step".PadRight(stepMaxWidth)}Duration");

    WriteLine($"{"".PadRight(stepMaxWidth - 15,'-')}{"".PadLeft(15)}{"".PadRight(18, '-')}");
    TimeSpan total = TimeSpan.Zero;
    foreach (var result in results.Reverse())
    {
        total = total.Add(result.Duration);
        WriteLine($"{result.Name.PadRight(stepMaxWidth)}{result.Duration.ToString()}");
    }
    WriteLine("---------------------------------------------------------------------");
    WriteLine($"{"Total".PadRight(stepMaxWidth)}{total.ToString()}");
}


private static class StepRunner
{
    private static object _submission;
    private static Type _submissionType;

    private static Stack<string> _callStack = new Stack<string>();

    private static List<StepResult> _results = new List<StepResult>();

    private static bool HasWrappedFields;


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static void Initialize(object submission)
    {
        _submission = submission;
        _submissionType = submission.GetType();

    }

    public async static Task Execute(IList<string> stepNames)
    {
        await ExecuteSteps(stepNames.ToArray());
    }

    private async static Task ExecuteSteps(string[] stepNames)
    {

         if (!HasWrappedFields)
         {
            WrapFields(_results);
            HasWrappedFields = true;
         }



        var stepDelegates = GetStepDelegates();

        if (stepDelegates.Keys.Intersect(stepNames).Count() == 0)
        {
            await GetDefaultDelegate(stepDelegates)();
        }

        foreach(var stepName in stepNames)
        {
            _callStack.Clear();

            if (stepName.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                stepDelegates.Values.ToArray().ShowHelp();
                continue;
            }

            if (stepDelegates.TryGetValue(stepName, out var stepDelegate))
            {
                await stepDelegate.Invoke();
                continue;
            }
        }

        GetSummaryStepDelegate()(_results);
        _results.Clear();
    }

    private static void WrapFields(List<StepResult> results)
    {
        WrapStepFields(results);
        WrapAsyncStepFields(results);
    }

    private static void WrapStepFields(List<StepResult> results)
    {
        var stepFields = GetStepFields<Step>();
        foreach(var stepField in stepFields)
        {
            var step = GetStepDelegate<Step>(stepField);
            Step wrappedStep = () =>
            {
                var stepresult = new StepResult(stepField.Name, TimeSpan.Zero);
                results.Add(stepresult);
                _callStack.Push(stepField.Name);
                var stopWatch = Stopwatch.StartNew();
                step();
                stopWatch.Stop();
                var durationForThisStep = stopWatch.Elapsed;
                _callStack.Pop();

                if (_callStack.Count > 0)
                {
                    var callingStep = results.Where(sr => sr.Name == _callStack.Peek()).Single();
                    callingStep.Duration = callingStep.Duration.Subtract(durationForThisStep);
                }

                results[results.IndexOf(stepresult)].Duration =results[results.IndexOf(stepresult)].Duration.Add(durationForThisStep);

            };
            stepField.SetValue(stepField.IsStatic ? null : _submission, wrappedStep);
        }
    }

    private static void WrapAsyncStepFields(List<StepResult> results)
    {
        var stepFields = GetStepFields<AsyncStep>();
        foreach(var stepField in stepFields)
        {
            var step = GetStepDelegate<AsyncStep>(stepField);
            AsyncStep wrappedStep = async () =>
            {
                var stopWatch = Stopwatch.StartNew();
                await step();
                // Do something with calling step to be able to report own time spent in this method.
                results.Add(new StepResult(stepField.Name, stopWatch.Elapsed));
            };
            stepField.SetValue(stepField.IsStatic ? null : _submission, wrappedStep);
        }
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

        return () =>
        {
            stepDelegates.Values.ToArray().ShowHelp();
            return Task.CompletedTask;
        };

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
        public TimeSpan Duration { get; set;}
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

    public async Task Invoke()
    {
        await _step();
    }
}