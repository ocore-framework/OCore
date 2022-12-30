using OCore.Entities.Data;

await OCore.Setup.Developer.LetsGo("Calculator");

public class CalculatorState
{
    public decimal Answer { get; set; }
}

[DataEntity("Calculator", dataEntityMethods: DataEntityMethods.All)]
public interface ICalculator : IDataEntity<CalculatorState>
{
    Task<decimal> Add(decimal value);
}

public class Calculator : DataEntity<CalculatorState>, ICalculator
{
    public Task<decimal> Add(decimal value)
    {
        State.Answer += value;
        return Task.FromResult(State.Answer);
    }
}