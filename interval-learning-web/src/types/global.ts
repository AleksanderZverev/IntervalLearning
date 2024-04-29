export interface Theme {
    id: number;
    name: string;
    languageId: string | null;
}

export class RememberAnswer {
    private weight: number;

    constructor(weight: number) {
        this.weight = weight;
    }

    public GetWeight = () => this.weight;
    public IsRemembered = () => this.weight != null && this.weight >= 0.8;
    public IsNotSure = () => this.weight != null && this.weight >= 0.4 && this.weight < 0.8;
    public IsForgotten = () => this.weight != null && this.weight < 0.4;
}
