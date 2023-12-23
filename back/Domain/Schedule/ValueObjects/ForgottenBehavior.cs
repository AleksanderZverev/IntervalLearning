namespace Domain.Schedule.ValueObjects;

public enum ForgottenBehavior
{
    MoveToNextStep = 1,
    StayOnCurrentStep = 2,
    MoveToPreviousStep = 3,
    StartFromFirstStep = 4,
}