import { User } from '@/types'

export interface AuthConfig {
  keycloakUrl: string
  realm: string
  clientId: string
  redirectUri: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  idToken: string
  expiresAt: number
}

export class AuthService {
  private config: AuthConfig
  private tokens: AuthTokens | null = null

  constructor(config: AuthConfig) {
    this.config = config
    this.loadTokensFromStorage()
  }

  private loadTokensFromStorage() {
    if (typeof window === 'undefined') return
    
    const stored = localStorage.getItem('auth_tokens')
    if (stored) {
      try {
        this.tokens = JSON.parse(stored)
      } catch (error) {
        console.error('Failed to parse stored tokens:', error)
        localStorage.removeItem('auth_tokens')
      }
    }
  }

  private saveTokensToStorage(tokens: AuthTokens) {
    if (typeof window === 'undefined') return
    localStorage.setItem('auth_tokens', JSON.stringify(tokens))
  }

  private clearTokensFromStorage() {
    if (typeof window === 'undefined') return
    localStorage.removeItem('auth_tokens')
  }

  getLoginUrl(): string {
    const params = new URLSearchParams({
      client_id: this.config.clientId,
      redirect_uri: this.config.redirectUri,
      response_type: 'code',
      scope: 'openid profile email',
      state: this.generateState()
    })

    return `${this.config.keycloakUrl}/realms/${this.config.realm}/protocol/openid-connect/auth?${params}`
  }

  getLogoutUrl(): string {
    const params = new URLSearchParams({
      client_id: this.config.clientId,
      post_logout_redirect_uri: this.config.redirectUri
    })

    return `${this.config.keycloakUrl}/realms/${this.config.realm}/protocol/openid-connect/logout?${params}`
  }

  private generateState(): string {
    const state = Math.random().toString(36).substring(2, 15)
    if (typeof window !== 'undefined') {
      sessionStorage.setItem('auth_state', state)
    }
    return state
  }

  private validateState(state: string): boolean {
    if (typeof window === 'undefined') return false
    const storedState = sessionStorage.getItem('auth_state')
    sessionStorage.removeItem('auth_state')
    return storedState === state
  }

  async handleCallback(code: string, state: string): Promise<AuthTokens> {
    if (!this.validateState(state)) {
      throw new Error('Invalid state parameter')
    }

    const tokenUrl = `${this.config.keycloakUrl}/realms/${this.config.realm}/protocol/openid-connect/token`
    
    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: new URLSearchParams({
        grant_type: 'authorization_code',
        client_id: this.config.clientId,
        code,
        redirect_uri: this.config.redirectUri,
      }),
    })

    if (!response.ok) {
      throw new Error('Failed to exchange code for tokens')
    }

    const data = await response.json()
    
    const tokens: AuthTokens = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      idToken: data.id_token,
      expiresAt: Date.now() + (data.expires_in * 1000)
    }

    this.tokens = tokens
    this.saveTokensToStorage(tokens)
    
    return tokens
  }

  async refreshTokens(): Promise<AuthTokens | null> {
    if (!this.tokens?.refreshToken) {
      return null
    }

    const tokenUrl = `${this.config.keycloakUrl}/realms/${this.config.realm}/protocol/openid-connect/token`
    
    try {
      const response = await fetch(tokenUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
          grant_type: 'refresh_token',
          client_id: this.config.clientId,
          refresh_token: this.tokens.refreshToken,
        }),
      })

      if (!response.ok) {
        throw new Error('Failed to refresh tokens')
      }

      const data = await response.json()
      
      const tokens: AuthTokens = {
        accessToken: data.access_token,
        refreshToken: data.refresh_token || this.tokens.refreshToken,
        idToken: data.id_token,
        expiresAt: Date.now() + (data.expires_in * 1000)
      }

      this.tokens = tokens
      this.saveTokensToStorage(tokens)
      
      return tokens
    } catch (error) {
      console.error('Token refresh failed:', error)
      this.logout()
      return null
    }
  }

  async getValidAccessToken(): Promise<string | null> {
    if (!this.tokens) {
      return null
    }

    // Check if token is expired (with 5 minute buffer)
    if (this.tokens.expiresAt - Date.now() < 5 * 60 * 1000) {
      const refreshed = await this.refreshTokens()
      if (!refreshed) {
        return null
      }
    }

    return this.tokens.accessToken
  }

  isAuthenticated(): boolean {
    return this.tokens !== null && this.tokens.expiresAt > Date.now()
  }

  logout() {
    this.tokens = null
    this.clearTokensFromStorage()
  }

  getTokens(): AuthTokens | null {
    return this.tokens
  }
}

// Create singleton instance
export const authService = new AuthService({
  keycloakUrl: process.env.NEXT_PUBLIC_KEYCLOAK_URL || 'http://localhost:8080',
  realm: process.env.NEXT_PUBLIC_KEYCLOAK_REALM || 'streetracer',
  clientId: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || 'streetracer-web',
  redirectUri: typeof window !== 'undefined' ? `${window.location.origin}/auth/callback` : 'http://localhost:3000/auth/callback'
})