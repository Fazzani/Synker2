/**
 * Authentification response
 * @description Authentification response.
 */
export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
}

/**
* User entity
* @description User entity
*/
export interface User {
    gender: string;
    firstName: string;
    lastName: string;
    email: string;
    birthday: Date;
    photo: string;
}

export interface RegisterUser extends User, Login {

    genders: any[];
}
/**
* Login Model
* 
* @export
* @class Login
*/
export interface Login {
    username: string;
    password: string;
}

export class AuthModel {
    public userName: string;
    public password: string;
    public refreshToken: string;
    public grantType: GrantType;
}
export enum GrantType {
    password = 0,
    refreshToken
}