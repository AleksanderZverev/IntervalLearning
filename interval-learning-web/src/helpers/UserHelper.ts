import { User, UserInfo } from './../types/user';
export class UserHelper {
    static getFullName(user: User | UserInfo) {
        return `${user.lastName} ${user.firstName}`.trim();
    }
}
