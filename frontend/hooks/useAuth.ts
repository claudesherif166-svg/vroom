'use client'

import { useState, useEffect, useCallback } from 'react'
import { authService } from '@/lib/auth'
import { apiClient } from '@/lib/api'
import { UserDto } from '@/types'

interface AuthState {
  isAuthenticated: boolean
  isLoading: boolean
  user: UserDto | null
  error: string | null
}

export function useAuth() {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    isLoading: true,
    user: null,
    error: null
  })

  const login = useCallback(() => {
    const loginUrl = authService.getLoginUrl()
    window.location.href = loginUrl
  }, [])

  const logout = useCallback(() => {
    authService.logout()
    setState({
      isAuthenticated: false,
      isLoading: false,
      user: null,
      error: null
    })
    
    // Redirect to logout URL
    const logoutUrl = authService.getLogoutUrl()
    window.location.href = logoutUrl
  }, [])

  const syncUser = useCallback(async () => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }))
      
      const user = await apiClient.syncUser()
      
      setState({
        isAuthenticated: true,
        isLoading: false,
        user,
        error: null
      })
      
      return user
    } catch (error) {
      console.error('Failed to sync user:', error)
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to sync user'
      }))
      return null
    }
  }, [])

  const refreshUser = useCallback(async () => {
    if (!authService.isAuthenticated()) {
      setState({
        isAuthenticated: false,
        isLoading: false,
        user: null,
        error: null
      })
      return
    }

    try {
      const user = await apiClient.getCurrentUser()
      setState(prev => ({
        ...prev,
        user,
        error: null
      }))
    } catch (error) {
      console.error('Failed to refresh user:', error)
      // Don't update error state for refresh failures
    }
  }, [])

  const handleCallback = useCallback(async (code: string, state: string) => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }))
      
      await authService.handleCallback(code, state)
      const user = await syncUser()
      
      return user
    } catch (error) {
      console.error('Auth callback failed:', error)
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Authentication failed'
      }))
      return null
    }
  }, [syncUser])

  // Initialize auth state
  useEffect(() => {
    const initAuth = async () => {
      if (authService.isAuthenticated()) {
        try {
          const user = await apiClient.getCurrentUser()
          setState({
            isAuthenticated: true,
            isLoading: false,
            user,
            error: null
          })
        } catch (error) {
          console.error('Failed to get current user:', error)
          // Try to sync user if current user fails
          try {
            const syncedUser = await apiClient.syncUser()
            setState({
              isAuthenticated: true,
              isLoading: false,
              user: syncedUser,
              error: null
            })
          } catch (syncError) {
            console.error('Failed to sync user:', syncError)
            authService.logout()
            setState({
              isAuthenticated: false,
              isLoading: false,
              user: null,
              error: null
            })
          }
        }
      } else {
        setState({
          isAuthenticated: false,
          isLoading: false,
          user: null,
          error: null
        })
      }
    }

    initAuth()
  }, [])

  return {
    ...state,
    login,
    logout,
    syncUser,
    refreshUser,
    handleCallback,
    getAccessToken: () => authService.getValidAccessToken()
  }
}