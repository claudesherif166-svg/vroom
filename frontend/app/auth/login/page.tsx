'use client'

import { useEffect } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { useRouter } from 'next/navigation'
import { Zap, ArrowRight } from 'lucide-react'

export default function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (isAuthenticated) {
      router.replace('/')
    }
  }, [isAuthenticated, router])

  const handleLogin = () => {
    // Store return URL
    const returnTo = new URLSearchParams(window.location.search).get('returnTo') || '/'
    sessionStorage.setItem('auth_return_to', returnTo)
    login()
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center">
          <div className="animate-spin w-16 h-16 border-4 border-race-primary border-t-transparent rounded-full mx-auto mb-4"></div>
          <p className="text-race-primary">Loading...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-race-dark via-race-gray to-race-dark flex items-center justify-center">
      <div className="racing-card p-8 max-w-md w-full mx-4">
        <div className="text-center mb-8">
          <div className="flex items-center justify-center space-x-2 mb-6">
            <Zap className="h-12 w-12 text-race-primary racing-pulse" />
            <span className="text-3xl font-racing font-bold text-race-primary">StreetRacer</span>
          </div>
          <h1 className="text-2xl font-bold mb-2">Welcome Back</h1>
          <p className="text-gray-400">Sign in to join the racing community</p>
        </div>

        <div className="space-y-6">
          <button
            onClick={handleLogin}
            className="racing-button w-full flex items-center justify-center space-x-2 text-lg py-4"
          >
            <span>Sign In with Keycloak</span>
            <ArrowRight className="w-5 h-5" />
          </button>

          <div className="text-center">
            <p className="text-sm text-gray-400">
              New to StreetRacer?{' '}
              <button
                onClick={handleLogin}
                className="text-race-primary hover:text-race-accent transition-colors"
              >
                Create an account
              </button>
            </p>
          </div>
        </div>

        <div className="mt-8 pt-6 border-t border-race-primary/20">
          <div className="text-center text-sm text-gray-500">
            <p>By signing in, you agree to our Terms of Service and Privacy Policy</p>
          </div>
        </div>
      </div>
    </div>
  )
}