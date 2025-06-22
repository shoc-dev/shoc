import { useIntl } from "react-intl"
import Icons from "../generic/icons";

export default function AuthLeftCard() {
  const intl = useIntl();

  return <div className="relative hidden h-full flex-col bg-muted p-10 text-white lg:flex dark:border-r">
    <div className="absolute inset-0 bg-zinc-900" />
    <div className="relative z-20 flex items-center text-lg font-medium">
      <Icons.logo className="h-6 w-6" />
      {intl.formatMessage({id: 'auth.platform'})}
    </div>
    <div className="relative z-20 mt-auto hidden">
      <blockquote className="space-y-2">
        <p className="text-lg">
          A cool quote in case if one day I will come up with it.
        </p>
        <footer className="text-sm">Me Myself</footer>
      </blockquote>
    </div>
  </div>
}