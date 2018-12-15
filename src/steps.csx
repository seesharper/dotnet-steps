
using System.Reflection;

public delegate void Step();

public delegate Task AsyncStep();

Action stepsDummyaction = () => StepsDummy();

WriteLine(stepsDummyaction.Target.GetType());

public void StepsDummy(){}


StepRunner.Initialize(stepsDummyaction.Target);



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

    public async static Task Execute(IList<string> stepNames, AsyncStep defaultStep = null)
    {
        if (stepNames.Count() == 0)
        {
            await defaultStep();
        }
        else
        {
            await ExecuteSteps(stepNames.ToArray());
        }
    }

    public async static Task Execute(IList<string> stepNames, Step defaultStep = null)
    {
        if (stepNames.Count() == 0)
        {
            defaultStep();
        }
        else
        {
            await ExecuteSteps(stepNames.ToArray());
        }
    }

    private async static Task ExecuteSteps(string[] stepNames)
    {
         var stepDelegates = GetStepDelegates<Step>();
        var asyncStepDelegates = GetStepDelegates<AsyncStep>();
        var results = new List<StepResult>();
        foreach(var stepName in stepNames)
        {
            var stepDelegate = stepDelegates.FirstOrDefault(sd => string.Equals(sd.Name, stepName, StringComparison.OrdinalIgnoreCase));
            if (stepDelegate != null)
            {
                stepDelegate.Step.Invoke();
            }
            else
            {
                var asyncStep = asyncStepDelegates.FirstOrDefault(sd => string.Equals(sd.Name, stepName, StringComparison.OrdinalIgnoreCase));
                if (asyncStep != null)
                {
                    await asyncStep.Step();
                }
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
        return new StepInfo<TStep>(property.Name, (TStep)(property.GetMethod.IsStatic ? property.GetValue(null) : property.GetValue(_submission)));
    }

    private static StepInfo<TStep> GetStepInfo<TStep>(FieldInfo field)
    {
        return new StepInfo<TStep>(field.Name,(TStep)(field.IsStatic ? field.GetValue(null) : field.GetValue(_submission)));
    }

    private static FieldInfo[] GetStepFields<TStep>()
    {
        return _submissionType.GetFields().Where(f => f.FieldType == typeof(TStep)).ToArray();
    }

    private static PropertyInfo[] GetStepProperties<TStep>()
    {
        return _submissionType.GetProperties().Where(f => f.PropertyType == typeof(TStep)).ToArray();
    }

    private class StepInfo<TStep>
    {
        public StepInfo(string name, TStep step)
        {
            Name = name;
            Step = step;
        }

        public string Name { get; }
        public TStep Step { get; }
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


    private class AsyncStepInfo : StepInfo<AsyncStep>
    {
        public AsyncStepInfo(string name, AsyncStep step) : base(name, step)
        {

        }
    }
}