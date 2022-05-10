export interface Schedule {
    userId: string;
    id: string;
    title: string;
    cardsCountPerPhase: number;
    shortDescription: string | null;
    description: string | null;
    defaultPhaseShortDescription: string | null;
    defaultPhaseDescription: string | null;
    defaultRepeatPhaseShortDescription: string | null;
    defaultRepeatPhaseDescription: string | null;
    isRecommended: boolean;
    forgottenBehavior: ForgottenBehavior;
    phases: PhaseInfo[];
}

export interface CreateScheduleItem {
    cardsCountPerPhase: number;
    forgottenBehavior: ForgottenBehavior;
    title: string;
    shortDescription: string | null;
    description: string | null;
    phases: PhaseInfo[];
    defaultPhaseShortDescription: string | null;
    defaultPhaseDescription: string | null;
    defaultRepeatPhaseShortDescription: string | null;
    defaultRepeatPhaseDescription: string | null;
}

export interface PhaseInfo {
    id: string;
    secondsFromLastPhase: number;
    shortDescription: string | null;
    description: string | null;
    isDefaultValueSide: boolean;
}

export enum ForgottenBehavior {
    MoveToNextStep = 1,
    StayOnCurrentStep = 2,
    MoveToPreviousStep = 3,
    StartFromFirstStep = 4,
}
