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
    Photo: string;
}

export class Login {
    constructor(email?: string, password?: string) { }
}