import { useEffect, useRef } from 'react'

export const CursorEffect = () => {
  const cursorRef    = useRef<HTMLDivElement>(null)
  const followerRef  = useRef<HTMLDivElement>(null)
  const pos          = useRef({ x: 0, y: 0 })
  const followerPos  = useRef({ x: 0, y: 0 })

  useEffect(() => {
    const cursor   = cursorRef.current
    const follower = followerRef.current
    if (!cursor || !follower) return

    const onMove = (e: MouseEvent) => {
      pos.current = { x: e.clientX, y: e.clientY }
      cursor.style.transform = `translate(${e.clientX - 6}px, ${e.clientY - 6}px)`
    }

    const onEnterLink = () => {
      cursor.classList.add('cursor--hover')
      follower.classList.add('follower--hover')
    }

    const onLeaveLink = () => {
      cursor.classList.remove('cursor--hover')
      follower.classList.remove('follower--hover')
    }

    let raf: number
    const animate = () => {
      followerPos.current.x += (pos.current.x - followerPos.current.x) * 0.1
      followerPos.current.y += (pos.current.y - followerPos.current.y) * 0.1
      follower.style.transform =
        `translate(${followerPos.current.x - 20}px, ${followerPos.current.y - 20}px)`
      raf = requestAnimationFrame(animate)
    }

    animate()
    window.addEventListener('mousemove', onMove)

    const addListeners = () => {
      document.querySelectorAll('a, button, .news-card, .category-item')
        .forEach(el => {
          el.addEventListener('mouseenter', onEnterLink)
          el.addEventListener('mouseleave', onLeaveLink)
        })
    }

    addListeners()
    const observer = new MutationObserver(addListeners)
    observer.observe(document.body, { childList: true, subtree: true })

    return () => {
      cancelAnimationFrame(raf)
      window.removeEventListener('mousemove', onMove)
      observer.disconnect()
    }
  }, [])

  return (
    <>
      <div ref={cursorRef}   className="cursor" />
      <div ref={followerRef} className="cursor-follower" />
    </>
  )
}