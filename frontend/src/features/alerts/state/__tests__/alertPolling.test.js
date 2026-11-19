import assert from 'node:assert/strict'
import { afterEach, test } from 'node:test'
import { startAlertPolling, ALERT_POLL_INTERVAL_MS } from '../alertPolling.js'

let restoreBrowser

afterEach(() => {
  if (restoreBrowser) {
    restoreBrowser()
    restoreBrowser = undefined
  }
})

function installFakeBrowser() {
  let nextTimerId = 0
  const timers = new Map()
  const windowListeners = new Map()
  const documentListeners = new Map()

  function addListener(listeners, eventName, listener) {
    const callbacks = listeners.get(eventName) ?? new Set()
    callbacks.add(listener)
    listeners.set(eventName, callbacks)
  }

  function removeListener(listeners, eventName, listener) {
    listeners.get(eventName)?.delete(listener)
  }

  const fakeWindow = {
    setTimeout(callback, delay) {
      const id = ++nextTimerId
      timers.set(id, { callback, delay })
      return id
    },
    clearTimeout(id) {
      timers.delete(id)
    },
    addEventListener(eventName, listener) {
      addListener(windowListeners, eventName, listener)
    },
    removeEventListener(eventName, listener) {
      removeListener(windowListeners, eventName, listener)
    },
    dispatch(eventName) {
      for (const listener of windowListeners.get(eventName) ?? []) {
        listener()
      }
    },
    async runNextTimer() {
      const [id, timer] = timers.entries().next().value ?? []
      assert.ok(timer, 'Expected a scheduled poll')
      timers.delete(id)
      await timer.callback()
    },
    get scheduledDelays() {
      return [...timers.values()].map(({ delay }) => delay)
    },
  }

  const fakeDocument = {
    visibilityState: 'visible',
    addEventListener(eventName, listener) {
      addListener(documentListeners, eventName, listener)
    },
    removeEventListener(eventName, listener) {
      removeListener(documentListeners, eventName, listener)
    },
    dispatch(eventName) {
      for (const listener of documentListeners.get(eventName) ?? []) {
        listener()
      }
    },
  }

  const originalWindow = globalThis.window
  const originalDocument = globalThis.document
  globalThis.window = fakeWindow
  globalThis.document = fakeDocument

  restoreBrowser = () => {
    if (originalWindow === undefined) {
      delete globalThis.window
    } else {
      globalThis.window = originalWindow
    }

    if (originalDocument === undefined) {
      delete globalThis.document
    } else {
      globalThis.document = originalDocument
    }
  }

  return { window: fakeWindow, document: fakeDocument }
}

test('refreshes every 10 seconds and retries after a failed refresh', async () => {
  const browser = installFakeBrowser()
  let refreshCount = 0
  const stopPolling = startAlertPolling(async () => {
    refreshCount += 1
    return false
  })

  assert.deepEqual(browser.window.scheduledDelays, [ALERT_POLL_INTERVAL_MS])

  await browser.window.runNextTimer()

  assert.equal(refreshCount, 1)
  assert.deepEqual(browser.window.scheduledDelays, [ALERT_POLL_INTERVAL_MS])
  stopPolling()
})

test('refreshes immediately on reconnect and when the tab becomes visible', async () => {
  const browser = installFakeBrowser()
  let refreshCount = 0
  const stopPolling = startAlertPolling(async () => {
    refreshCount += 1
    return true
  })

  browser.window.dispatch('online')
  await new Promise((resolve) => setImmediate(resolve))
  assert.equal(refreshCount, 1)
  assert.deepEqual(browser.window.scheduledDelays, [ALERT_POLL_INTERVAL_MS])

  browser.document.visibilityState = 'hidden'
  browser.document.dispatch('visibilitychange')
  assert.deepEqual(browser.window.scheduledDelays, [])

  browser.document.visibilityState = 'visible'
  browser.document.dispatch('visibilitychange')
  await new Promise((resolve) => setImmediate(resolve))

  assert.equal(refreshCount, 2)
  assert.deepEqual(browser.window.scheduledDelays, [ALERT_POLL_INTERVAL_MS])
  stopPolling()
})
