'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { apiClient } from '@/lib/api'
import { PostDto, PagedResult } from '@/types'
import { useRouter } from 'next/navigation'
import { Zap, Heart, MessageCircle, Share, Plus } from 'lucide-react'
import Link from 'next/link'

export default function FeedPage() {
  const { isAuthenticated, isLoading, user } = useAuth()
  const router = useRouter()
  const [posts, setPosts] = useState<PostDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [hasMore, setHasMore] = useState(false)

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/auth/login?returnTo=/feed')
      return
    }

    if (isAuthenticated) {
      loadFeed()
    }
  }, [isAuthenticated, isLoading, router])

  const loadFeed = async (cursor?: string) => {
    try {
      setError(null)
      const result = await apiClient.getFeed(cursor)
      
      if (cursor) {
        setPosts(prev => [...prev, ...result.items])
      } else {
        setPosts(result.items)
      }
      
      setNextCursor(result.nextCursor || null)
      setHasMore(result.hasMore)
    } catch (error) {
      console.error('Failed to load feed:', error)
      setError(error instanceof Error ? error.message : 'Failed to load feed')
    } finally {
      setLoading(false)
    }
  }

  const loadMore = () => {
    if (nextCursor && !loading) {
      setLoading(true)
      loadFeed(nextCursor)
    }
  }

  const handleLike = async (postId: string, isLiked: boolean) => {
    try {
      if (isLiked) {
        await apiClient.unlikePost(postId)
      } else {
        await apiClient.likePost(postId)
      }
      
      // Update local state
      setPosts(prev => prev.map(post => 
        post.id === postId 
          ? { 
              ...post, 
              isLikedByCurrentUser: !isLiked,
              likesCount: post.likesCount + (isLiked ? -1 : 1)
            }
          : post
      ))
    } catch (error) {
      console.error('Failed to toggle like:', error)
    }
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
    <div className="min-h-screen bg-race-dark">
      {/* Header */}
      <header className="bg-race-gray/50 backdrop-blur-sm border-b border-race-primary/20 p-4">
        <div className="container mx-auto flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Link href="/" className="flex items-center space-x-2">
              <Zap className="h-8 w-8 text-race-primary" />
              <span className="text-2xl font-racing font-bold text-race-primary">StreetRacer</span>
            </Link>
          </div>

          <div className="flex items-center space-x-4">
            <span className="text-race-primary">Welcome, {user?.username}</span>
            <Link href="/races/new" className="racing-button flex items-center space-x-2">
              <Plus className="w-4 h-4" />
              <span>New Race</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="container mx-auto p-4 max-w-2xl">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">Your Feed</h1>
          <p className="text-gray-400">Stay updated with the racing community</p>
        </div>

        {error && (
          <div className="racing-card p-4 mb-6 border-red-500/50 bg-red-500/10">
            <p className="text-red-400">{error}</p>
            <button 
              onClick={() => loadFeed()}
              className="mt-2 text-race-primary hover:text-race-accent transition-colors"
            >
              Try again
            </button>
          </div>
        )}

        {/* Posts */}
        <div className="space-y-6">
          {posts.length === 0 && !loading ? (
            <div className="racing-card p-8 text-center">
              <Zap className="w-16 h-16 text-race-primary/50 mx-auto mb-4" />
              <h3 className="text-xl font-bold mb-2">No posts yet</h3>
              <p className="text-gray-400 mb-4">
                Follow other racers or create your first post to get started!
              </p>
              <Link href="/explore" className="racing-button">
                Explore Community
              </Link>
            </div>
          ) : (
            posts.map((post) => (
              <div key={post.id} className="racing-card">
                {/* Post Header */}
                <div className="flex items-center space-x-3 mb-4">
                  <div className="w-10 h-10 bg-race-primary rounded-full flex items-center justify-center">
                    <span className="text-race-dark font-bold">
                      {post.authorUsername.charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div className="flex-1">
                    <h4 className="font-semibold">{post.authorUsername}</h4>
                    <p className="text-sm text-gray-400">
                      {new Date(post.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>

                {/* Post Content */}
                {post.text && (
                  <div className="mb-4">
                    <p className="text-gray-200">{post.text}</p>
                  </div>
                )}

                {/* Post Media */}
                {post.media.length > 0 && (
                  <div className="mb-4 grid grid-cols-1 gap-2">
                    {post.media.map((media) => (
                      <div key={media.id} className="rounded-lg overflow-hidden">
                        {media.mediaType === 'image' ? (
                          <img 
                            src={media.mediaUrl} 
                            alt="Post media"
                            className="w-full h-auto"
                          />
                        ) : (
                          <video 
                            src={media.mediaUrl}
                            controls
                            className="w-full h-auto"
                          />
                        )}
                      </div>
                    ))}
                  </div>
                )}

                {/* Post Actions */}
                <div className="flex items-center justify-between pt-4 border-t border-race-primary/20">
                  <div className="flex items-center space-x-6">
                    <button
                      onClick={() => handleLike(post.id, post.isLikedByCurrentUser)}
                      className={`flex items-center space-x-2 transition-colors ${
                        post.isLikedByCurrentUser 
                          ? 'text-race-secondary' 
                          : 'text-gray-400 hover:text-race-secondary'
                      }`}
                    >
                      <Heart className={`w-5 h-5 ${post.isLikedByCurrentUser ? 'fill-current' : ''}`} />
                      <span>{post.likesCount}</span>
                    </button>

                    <button className="flex items-center space-x-2 text-gray-400 hover:text-race-accent transition-colors">
                      <MessageCircle className="w-5 h-5" />
                      <span>{post.commentsCount}</span>
                    </button>

                    <button className="flex items-center space-x-2 text-gray-400 hover:text-race-primary transition-colors">
                      <Share className="w-5 h-5" />
                      <span>Share</span>
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Load More */}
        {hasMore && (
          <div className="mt-8 text-center">
            <button
              onClick={loadMore}
              disabled={loading}
              className="racing-button disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Loading...' : 'Load More'}
            </button>
          </div>
        )}
      </div>
    </div>
  )
}