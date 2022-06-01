export class EnvironmentHelper {
    static IsDevelopment(): boolean {
        return process.env.NODE_ENV === 'development';
    }

    static IsProduction(): boolean {
        return process.env.NODE_ENV === 'production';
    }
}
