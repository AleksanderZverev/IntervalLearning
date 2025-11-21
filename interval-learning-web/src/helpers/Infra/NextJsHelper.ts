export class NextJsHelper {
    static isServerSide(): boolean {
        return typeof window === 'undefined';
    }
}
