'use client'

import { useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { useAuth } from '@/hooks/useAuth'
import { Zap } from 'lucide-react'

export default function AuthCallbackPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { handleCallback } = useAuth()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const processCallback = async () => {
      const code = searchParams.get('code')
      const state = searchParams.get('state')
      const errorParam = searchParams.get('error')

      if (errorParam) {
        setError(`Authentication failed: ${errorParam}`)
        return
      }

      if (!code || !state) {
        setError('Missing required parameters')
        return
      }

      try {
        const user = await handleCallback(code, state)
        if (user) {
          // Redirect to intended page or home
          const returnTo = sessionStorage.getItem('auth_return_to') || '/'
          sessionStorage.removeItem('auth_return_to')
          router.replace(returnTo)
        }
      } catch (error) {
        console.error('Callback processing failed:', error)
        setError(error instanceof Error ? error.message : 'Authentication failed')
      }
    }

    processCallback()
  }, [searchParams, handleCallback, router])

  if (error) {
    return (
      <div className="min-h-screen bg-race-dark flex items-center justify-center">
        <div className="racing-card p-8 text-center max-w-md">
          <div className="w-16 h-16 bg-red-500 rounded-full flex items-center justify-center mx-auto mb-4">
            <span className="text-2xl">⚠️</span>
          </div>
          <h1 className="text-2xl font-bold mb-4 text-red-400">Authentication Failed</h1>
          <p className="text-gray-300 mb-6">{error}</p>
          <button
            onClick={() => router.push('/')}
            className="racing-button"
          >
            Return Home
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-race-dark flex items-center justify-center">
      <div className="racing-card p-8 text-center">
        <div className="racing-pulse mb-6">
          <Zap className="w-16 h-16 text-race-primary mx-auto" />
        </div>
        <h1 className="text-2xl font-bold mb-4">Completing Sign In</h1>
        <p className="text-gray-400">Please wait while we set up your account...</p>
        <div className="mt-6">
          <div className="animate-spin w-8 h-8 border-4 border-race-primary border-t-transparent rounded-full mx-auto"></div>
        </div>
      </div>
    </div>
  )
}