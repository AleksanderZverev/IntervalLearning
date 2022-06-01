import { User } from './../types/user';
export class UserHelper {
    static getFullName(user: User) {
        return `${user.lastName} ${user.firstName}`.trim();
    }
}
