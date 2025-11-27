import { Remember } from '../../types/Collection';
import { PhaseInfo } from '../../types/schedule';
import _ from 'lodash';

export class PhaseHelper {
    static isRepeatingPhase(phase: PhaseInfo): boolean {
        return phase.secondsFromLastPhase < 10;
    }

    static GetCurrentPhaseIdFromRemembers(remembers: Remember[]): number {
        if (!remembers || remembers.length === 0) return 1;

        const lastPhaseIndex = _.last(remembers)!.phaseIndex;
        const currentPhaseId: number = lastPhaseIndex < 0 ? 1 : lastPhaseIndex + 1;
        return currentPhaseId;
    }
}
