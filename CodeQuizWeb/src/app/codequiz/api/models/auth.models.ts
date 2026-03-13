import { UserDTO } from './user.models';

export interface RegisterModel {
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  password: string;
}

export interface LoginModel {
  username: string;
  password: string;
}

export interface LoginResult {
  user: UserDTO;
  tokenModel: TokenModel;
}

export interface TokenModel {
  accessToken: string;
  refreshToken: string;
}

export interface ResetPasswordModel {
  username: string;
  password: string;
  newPassword: string;
}

export interface ForgetPasswordModel {
  email: string;
}

export interface ResetPasswordTnModel {
  email: string;
  token: string;
  newPassword: string;
}
