namespace DB.Models;

public enum ForgottenBehavior
{
    MoveToNextStep = 0,
    StayOnCurrentStep = 1,
    MoveToPreviousStep = 3,
    StartFromFirstStep = 4,
}