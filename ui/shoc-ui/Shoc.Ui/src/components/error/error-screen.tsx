"use client"

import { useIntl } from "react-intl";
import { ErrorKind } from "@/addons/error-handling/error-types";
import { IntlMessageId } from "@/i18n/sources";
import { ReactNode } from "react";
import { cn } from "@/lib/utils";

const titles: Record<ErrorKind, IntlMessageId> = {
  'not_found': 'errors.kinds.not_found.title',
  'data': 'errors.kinds.data.title',
  'validation': 'errors.kinds.validation.title',
  'access_denied': 'errors.kinds.access_denied.title',
  'not_authenticated': 'errors.kinds.not_authenticated.title',
  'not_allowed': 'errors.kinds.not_allowed.title',
  'unknown': 'errors.kinds.unknown.title'
}

const descriptions: Record<ErrorKind, IntlMessageId> = {
  'not_found': 'errors.kinds.not_found.description',
  'data': 'errors.kinds.data.description',
  'validation': 'errors.kinds.validation.description',
  'access_denied': 'errors.kinds.access_denied.description',
  'not_authenticated': 'errors.kinds.not_authenticated.description',
  'not_allowed': 'errors.kinds.not_allowed.description',
  'unknown': 'errors.kinds.unknown.description'
}

export default function ErrorScreen({ errors, kind: givenKind, title: givenTitle, description: givenDescription, children }: { errors?: any[] | null, kind?: ErrorKind, title?: string, description?: string, children?: ReactNode }) {
  const intl = useIntl();
  const kind: ErrorKind = givenKind || (errors || [])[0]?.kind as ErrorKind || 'unknown';

  const title = givenTitle || intl.formatMessage({ id: titles[kind] });
  const description = givenDescription || intl.formatMessage({ id: descriptions[kind] });


  return (
    <div className="flex h-full w-full flex-col items-center justify-center bg-background text-card-foreground">
      <section className={cn("bg-white dark:bg-gray-900")}>
          <div className="px-4 mx-auto text-center lg:px-12">
            <h1 className="text-center mb-4 text-2xl font-bold tracking-tight leading-none text-gray-900 lg:mb-6 md:text-3xl xl:text-5xl dark:text-white">
              {title}
            </h1>
            <p className="text-center font-light text-gray-500 md:text-lg xl:text-xl dark:text-gray-400">
              {description}
            </p>
            {children && <div>
              {children}
            </div>}
          </div>
        </section>
    </div>
  )
}