export class ArrayHelper {
    static shuffleArray<T>(array: T[]): void {
        if (array.length === 0 || array.length === 1) {
            return;
        }

        const needCheck = array.length < 10;
        const oldArray = needCheck ? [...array] : [];

        while (true) {
            for (let i = array.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [array[i], array[j]] = [array[j], array[i]];
            }

            if (!needCheck) {
                break;
            }

            const equals = this.arraysEqual(oldArray, array);
            console.log('eq: ', equals);
            if (!equals) {
                break;
            }
        }
    }

    static arraysEqual<T>(a: T[] | null, b: T[] | null) {
        if (a === b) return true;
        if (a == null || b == null) return false;
        if (a.length !== b.length) return false;

        // If you don't care about the order of the elements inside
        // the array, you should sort both arrays here.
        // Please note that calling sort on an array will modify that array.
        // you might want to clone your array first.

        for (var i = 0; i < a.length; ++i) {
            if (a[i] !== b[i]) return false;
        }
        return true;
    }
}
