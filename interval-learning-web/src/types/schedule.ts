export interface Schedule {
    userId: string;
    id: number;
    title: string;
    description: string | null;
    cardsCountPerPhase: number;
    forgottenBehavior: ForgottenBehavior;
    phases: PhaseInfo[];
}

export interface Phase {
    userId: string;
    scheduleId: string;
    id: string;
    secondsFromLastPhase: number;
    description: string | null;
}

export interface CreateScheduleItem {
    cardsCountPerPhase: number;
    forgottenBehavior: ForgottenBehavior;
    title: string;
    description: string | null;
    phases: PhaseInfo[];
}

export interface PhaseInfo {
    id: number;
    secondsFromLastPhase: number;
    description: string | null;
}

export enum ForgottenBehavior {
    MoveToNextStep = 1,
    StayOnCurrentStep = 2,
    MoveToPreviousStep = 3,
    StartFromFirstStep = 4,
}
