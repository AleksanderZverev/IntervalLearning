import { EnvironmentHelper } from '../helpers/EnvironmentHelper';

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

export function getForgottenBehaviorTitle(behavior: ForgottenBehavior): string {
    switch (behavior) {
        case ForgottenBehavior.MoveToNextStep:
            return 'Перейти на следующий этап';
        case ForgottenBehavior.MoveToPreviousStep:
            return 'Перейти на предыдущий этап';
        case ForgottenBehavior.StartFromFirstStep:
            return 'Перейти на первый этап';
        case ForgottenBehavior.StayOnCurrentStep:
            return 'Остаться на текущем этапе';
        default: {
            if (EnvironmentHelper.IsDevelopment()) {
                throw new Error('Unknown forgotten behavior');
            } else {
                return 'Неизвестный тип';
            }
        }
    }
}
