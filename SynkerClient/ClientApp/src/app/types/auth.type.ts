import * as moment from 'moment';
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
export class User {
  id: number;
  gender: string;
  firstName: string;
  lastName: string;
  email: string;
  emailHash: string;
  birthdate: Date;
  photo: string | ArrayBuffer;
  roles: roles = "Default";
  public static GENDERS = ["Mr", "Mrs"];

  public static FromUserProfile(userProfile: any): User {
    if (userProfile != null) {
      return <User>{
        email: userProfile.email,
        firstName: userProfile.given_name,
        lastName: userProfile.name,
        photo: userProfile.picture,
        gender: userProfile.gender,
        birthdate: moment(userProfile.birthdate, "DD-MM-YYYY").toDate(),
        roles: userProfile.role,
        id: userProfile.sub,
        emailHash: userProfile.email_hash
      }
    }
    return null;
  }
}

export type roles = "Default" | "Guest" | "Administrators";

export interface RegisterUser extends User, Login {
  confirmPassword: string;
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
