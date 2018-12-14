
using System.Reflection;

public delegate void Step();

public delegate Task AsyncStep();

Action action = () => Dummy();

WriteLine(action.Target.GetType());

public void Dummy(){}


StepRunner.Initialize(action.Target);



public static class StepRunner
{
    private static object _submission;
    private static Type _submissionType;

    internal static void Initialize(object submission)
    {
        _submission = submission;
        _submissionType = submission.GetType();
    }

    public async static Task<int> Execute(IList<string> args, Step defaultStep = null)
    {
        var stepDelegates = GetStepDelegates<Step>();
        var asyncStepDelegates = GetStepDelegates<AsyncStep>();


        defaultStep = stepDelegates.First().Step;
        defaultStep();

        return 0;
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
}