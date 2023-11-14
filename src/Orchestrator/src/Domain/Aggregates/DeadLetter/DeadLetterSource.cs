namespace Domain.Aggregates;

public enum DeadLetterSource
{
    ActionableEventFromKafka,
    TaskSystem,
}
