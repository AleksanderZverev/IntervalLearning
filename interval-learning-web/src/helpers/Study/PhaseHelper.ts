import { PhaseInfo } from '../../types/schedule';

export class PhaseHelper {
    //TODO: move responsibility to API
    static isRepeatingPhase(phase: PhaseInfo): boolean {
        return phase.secondsFromLastPhase < 10;
    }
}
